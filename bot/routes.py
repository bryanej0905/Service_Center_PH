import os
import logging
from flask import Blueprint, request, jsonify, render_template
from utils import normalize_text
from difflib import get_close_matches

# Lista de palabras prohibidas (profano)
PROHIBITED_WORDS = {'pinga', 'puta', 'cabron', 'mierda'}
# Lista de temas completamente prohibidos
PROHIBITED_TOPICS = {'bomba', 'explosivo', 'arma'}

# Umbral mínimo de similitud para aceptar una respuesta
EMBED_THRESHOLD = 0.6

def contains_profanity(text: str) -> bool:
    tokens = set(text.split())
    return any(word in PROHIBITED_WORDS for word in tokens)

def contains_disallowed_topic(text: str) -> bool:
    tokens = set(text.split())
    return any(topic in tokens for topic in PROHIBITED_TOPICS)

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

tchat_bp = Blueprint('chat', __name__)
logger   = setup_logger()

# Estos objetos serán inyectados desde app.py
faq_loader = None
matcher    = None

@tchat_bp.route('/')
def index():
    return render_template('index.html')


@tchat_bp.route('/upload_csv', methods=['POST'])
def upload_csv():
    global faq_loader  # usamos el loader que ya inyectaste en app.py

    file = request.files.get('file')
    if not file or not file.filename.endswith('.csv'):
        return "Archivo inválido. Debe ser .csv", 400

    # Guardar el archivo en /data
    upload_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'data')
    os.makedirs(upload_dir, exist_ok=True)
    path = os.path.join(upload_dir, file.filename)
    file.save(path)

    # Recargar todos los CSVs del directorio
    faq_loader.reload()

    return f'Archivo {file.filename} subido correctamente y FAQs actualizados.', 200


@tchat_bp.route('/available_csvs', methods=['GET'])
def available_csvs():
    data_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'data')
    files = [f for f in os.listdir(data_dir) if f.endswith('.csv')]
    return jsonify(files)

@tchat_bp.route('/delete_csv', methods=['DELETE'])
def delete_csv():
    global faq_loader

    # Verifica faq_loader y data_dir
    if not faq_loader:
        return jsonify({'success': False, 'error': 'Servicio no inicializado.'}), 500

    data_dir = getattr(faq_loader, 'data_dir', None)
    if not data_dir or not os.path.isdir(data_dir):
        return jsonify({'success': False, 'error': 'Directorio no encontrado.'}), 400

    data = request.get_json(force=True, silent=True)
    if not data or 'filename' not in data:
        return jsonify({'success': False, 'error': 'Falta el nombre del archivo.'}), 400

    filename = data['filename']

    # Seguridad básica contra path traversal
    if '/' in filename or '\\' in filename or '..' in filename:
        return jsonify({'success': False, 'error': 'Nombre de archivo inválido.'}), 400

    if not filename.endswith('.csv'):
        return jsonify({'success': False, 'error': 'Archivo debe ser .csv'}), 400

    file_path = os.path.join(data_dir, filename)

    if not os.path.isfile(file_path):
        return jsonify({'success': False, 'error': 'Archivo no existe.'}), 404

    try:
        os.remove(file_path)
        faq_loader.reload()  # Recarga los datos para que refleje el cambio
        return jsonify({'success': True})
    except Exception as e:
        return jsonify({'success': False, 'error': str(e)}), 500


@tchat_bp.errorhandler(Exception)
def handle_exception(e):
    # Aquí puedes loguear la excepción e
    return jsonify({'success': False, 'error': str(e)}), 500

@tchat_bp.route('/chat', methods=['POST'])
def chat():
    raw = request.json.get('message', '') or ''
    q_norm = normalize_text(raw)

    # 1) Filtros previos…
    if contains_disallowed_topic(q_norm) or contains_profanity(q_norm):
        return jsonify({'response':'Lo siento, no puedo ayudar con esa solicitud.','suggestions':[]})
    if not q_norm:
        return jsonify({'response':'Por favor, escribe una pregunta válida.','suggestions':[]})

    # 2) Exact match directo
    if q_norm in faq_loader._q2a:
        # sugerencias semánticas
        answer, suggestions, _ = (*matcher.find(q_norm), 1.0)[:3]
        return jsonify({'response':faq_loader._q2a[q_norm],'suggestions':suggestions})

    # 3) EmbeddingMatcher
    answer, suggestions, score = matcher.find(q_norm)

    # 4) Si el embedding es suficiente
    if score >= EMBED_THRESHOLD:
        return jsonify({'response':answer,'suggestions':suggestions})

    # 5) **Fallback difflib**: buscar la clave más parecida
    # Aquí bajamos el cutoff un poco para capturar "nutricion" ↔ "nutricional"
    close = get_close_matches(q_norm, list(faq_loader._q2a.keys()), n=1, cutoff=0.5)
    if close:
        resp = faq_loader._q2a[close[0]]
        return jsonify({'response':resp,'suggestions':[]})

    # 6) Si nada funciona, mensaje de “no info”
    logger.info(f"No match (score={score:.2f}) for: '{raw}'")
    return jsonify({'response':'Lo siento, no tengo la información suficiente para eso.','suggestions':[]})
