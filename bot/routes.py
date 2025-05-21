import os
import logging
from flask import Blueprint, request, jsonify, render_template
from utils import normalize_text
from gpt_fallback import generate_rag_response

# Lista de palabras prohibidas (profano)
PROHIBITED_WORDS = {'pinga', 'puta', 'cabron', 'mierda'}
# Lista de temas completamente prohibidos
PROHIBITED_TOPICS = {'bomba', 'explosivo', 'arma'}

# Umbral mínimo de similitud para permitir fallback RAG
RAG_THRESHOLD = 0.6


def contains_profanity(text: str) -> bool:
    """True si aparece alguna palabra profana en el texto."""
    tokens = set(text.split())
    return any(word in PROHIBITED_WORDS for word in tokens)


def contains_disallowed_topic(text: str) -> bool:
    """True si el texto menciona un tema completamente prohibido."""
    tokens = set(text.split())
    return any(topic in tokens for topic in PROHIBITED_TOPICS)


# Configuración del logger para consultas sin respuesta
def setup_logger():
    log_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'logs')
    os.makedirs(log_dir, exist_ok=True)
    log_path = os.path.join(log_dir, 'unanswered.log')

    logging.basicConfig(
        filename=log_path,
        format='%(asctime)s %(message)s',
        level=logging.INFO
    )
    return logging.getLogger(__name__)


# Creamos el blueprint
tchat_bp = Blueprint('chat', __name__)
logger = setup_logger()

# Objetos inyectados desde app.py
faq_loader = None
matcher = None


@tchat_bp.route('/')
def index():
    return render_template('index.html')


@tchat_bp.route('/chat', methods=['POST'])
def chat():
    raw = request.json.get('message', '') or ''
    # Normalización para filtros y búsqueda
    q_norm = normalize_text(raw)

    # 1) Prohibir temas sensibles
    if contains_disallowed_topic(q_norm):
        return jsonify({'response': 'Lo siento, no puedo ayudar con esa solicitud.', 'suggestions': []})

    # 2) Filtro de profanidad
    if contains_profanity(q_norm):
        return jsonify({'response': 'Lo siento, no puedo ayudar con esa solicitud.', 'suggestions': []})

    # 3) Pregunta vacía
    if not q_norm:
        return jsonify({'response': 'Por favor, escribe una pregunta válida.', 'suggestions': []})

    # 4) Intento de matcher con score
    result = matcher.find(q_norm)
    if len(result) == 3:
        answer, suggestions, score = result
    else:
        answer, suggestions = result
        score = 1.0  # confianza alta por defecto

    # 5) Respuesta directa si existe
    if answer:
        return jsonify({'response': answer, 'suggestions': suggestions})

    # 6) Si similitud baja, no fallback
    if score < RAG_THRESHOLD:
        logger.info(f"Similarity {score:.2f} below threshold for: '{raw}'")
        return jsonify({
            'response': 'Lo siento, no tengo la información suficiente para eso.',
            'suggestions': []
        })

    # 7) Loguear la consulta sin respuesta original
    logger.info(f"No answer for: '{raw}'")

    # 8) Preparar contexto para RAG
    context_pairs = [(q, faq_loader.get_answer(q)) for q in suggestions]
    rag_response = generate_rag_response(raw, context_pairs)

    return jsonify({'response': rag_response, 'suggestions': suggestions})
