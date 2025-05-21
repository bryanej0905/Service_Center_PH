import os
import logging
from flask import Blueprint, request, jsonify, render_template
from utils import normalize_text
from gpt_fallback import generate_rag_response

# Lista de palabras prohibidas (en minúsculas, sin acentos)
PROHIBITED_WORDS = {'pinga', 'puta', 'cabron', 'mierda'}  # Añade más según necesidad

# Umbral mínimo de similitud para permitir fallback RAG
RAG_THRESHOLD = 0.6

def contains_profanity(text: str) -> bool:
    """
    Devuelve True si alguno de los términos prohibidos aparece en el texto normalizado.
    """
    tokens = set(text.split())
    return any(word in PROHIBITED_WORDS for word in tokens)

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
    q_norm = normalize_text(raw)

    # Filtro de profanidad
    if contains_profanity(q_norm):
        return jsonify({'response': 'Lo siento, no puedo ayudar con esa solicitud.', 'suggestions': []})

    if not q_norm:
        return jsonify({'response': 'Por favor, escribe una pregunta válida.', 'suggestions': []})

    # Intento de respuesta y captura de score desde el matcher
    # Se espera que matcher.find devuelva (answer, suggestions, score)
    result = matcher.find(q_norm)
    if len(result) == 3:
        answer, suggestions, score = result
    else:
        # Caída para compatibilidad: sin score
        answer, suggestions = result
        score = 1.0  # asume alta confianza

    if answer:
        return jsonify({'response': answer, 'suggestions': suggestions})

    # Si la similitud es muy baja, no invocar al LLM
    if score < RAG_THRESHOLD:
        logger.info(f"Similarity {score:.2f} below threshold for: '{raw}'")
        return jsonify({
            'response': 'Lo siento, no tengo la información suficiente para eso.',
            'suggestions': []
        })

    # Loguear la consulta sin respuesta original
    logger.info(f"No answer for: '{raw}'")

    # Preparar contexto para RAG: usar las sugerencias como contexto
    context_pairs = [(q, faq_loader.get_answer(q)) for q in suggestions]
    rag_response = generate_rag_response(raw, context_pairs)

    return jsonify({'response': rag_response, 'suggestions': suggestions})
