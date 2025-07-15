
import os
import logging
import requests
from flask import Blueprint, request, jsonify, render_template, session
from utils import normalize_text
from difflib import get_close_matches
import spacy
import base64
from embedding_matcher import EmbeddingMatcher  # Asegúrate de que la ruta y el nombre del archivo sean correctos

# ======================= Configuración inicial =========================

PROHIBITED_WORDS = {'pinga', 'puta', 'cabron', 'mierda'}
PROHIBITED_TOPICS = {'bomba', 'explosivo', 'arma'}
EMBED_THRESHOLD = 0.6

# NLP para sustantivos en español
nlp = spacy.load("es_core_news_sm")

# Logger
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

logger = setup_logger()
tchat_bp = Blueprint('chat', __name__)

# Inyectados desde app.py
faq_loader = None
matcher = None

# ======================= Utilidades =========================

def contains_profanity(text: str) -> bool:
    tokens = set(text.split())
    return any(word in PROHIBITED_WORDS for word in tokens)

def contains_disallowed_topic(text: str) -> bool:
    tokens = set(text.split())
    return any(topic in PROHIBITED_TOPICS for topic in tokens)

def extract_nouns(texts):
    nouns = []
    for text in texts:
        doc = nlp(text)
        nouns.extend([token.text.capitalize() for token in doc if token.pos_ == "NOUN"])
    return list(dict.fromkeys(nouns))


def generar_titulo_ticket(nouns):
    return ", ".join(nouns)

def login_api(username, password):
    try:
        response = requests.post("https://api.servicecenterph.com/api/Auth/login", json={
            "username": username,
            "password": password
        }, timeout=5)
        if response.status_code == 200:
            return response.json().get("token")
        return None
    except requests.exceptions.RequestException as e:
        logger.error(f"Error al hacer login: {e}")
        return None

def enviar_ticket(nombre, titulo, descripcion, categoria, token):
    headers = {"Authorization": f"Bearer {token}"}
    payload = {
        "nombre": nombre,
        "titulo": titulo,
        "descripcion": descripcion,
        "categoria": categoria
    }
    try:
        response = requests.post("https://api.servicecenterph.com/api/Tickets", headers=headers, json=payload, timeout=5)
        return response.status_code, response.text
    except requests.exceptions.RequestException as e:
        logger.error(f"Error al enviar ticket: {e}")
        return 500, "Error al conectar con el servidor de tickets."

# ======================= Rutas =========================

@tchat_bp.route('/')
def index():
    return render_template('index.html')

@tchat_bp.route('/chat', methods=['POST'])
def chat():
    global faq_loader, matcher  # Asegura que usamos los objetos globales

    # Recarga los datos y el matcher (con nuevos embeddings)
    faq_loader.reload()
    matcher = EmbeddingMatcher(faq_loader.questions, faq_loader.get_answer)

    raw = request.json.get('message', '') or ''
    q_norm = normalize_text(raw)

    session.setdefault('recent_inputs', [])
    session['recent_inputs'].append(raw)
    if len(session['recent_inputs']) > 3:
        session['recent_inputs'].pop(0)

    if contains_disallowed_topic(q_norm) or contains_profanity(q_norm):
        return jsonify({'response': 'Lo siento, no puedo ayudar con esa solicitud.', 'suggestions': []})

    if not q_norm:
        return jsonify({'response': 'Por favor, escribe una pregunta válida.', 'suggestions': []})

    if q_norm in faq_loader._q2a:
        answer, suggestions, _ = (*matcher.find(q_norm), 1.0)[:3]
        session['intentos_fallidos'] = 0
        return jsonify({'response': faq_loader._q2a[q_norm], 'suggestions': suggestions})

    answer, suggestions, score = matcher.find(q_norm)

    if score >= EMBED_THRESHOLD:
        session['intentos_fallidos'] = 0
        return jsonify({'response': answer, 'suggestions': suggestions})

    close = get_close_matches(q_norm, list(faq_loader._q2a.keys()), n=1, cutoff=0.5)
    if close:
        session['intentos_fallidos'] = 0
        resp = faq_loader._q2a[close[0]]
        return jsonify({'response': resp, 'suggestions': []})

    logger.info(f"No match (score={score:.2f}) for: '{raw}'")
    session['intentos_fallidos'] = session.get('intentos_fallidos', 0) + 1

    if session['intentos_fallidos'] >= 3:
        recent = session.get('recent_inputs', [])
        nouns = extract_nouns(recent)
        titulo = generar_titulo_ticket(nouns)
        return jsonify({
            'response': 'No encontré una solución. ¿Deseas crear un ticket?',
            'ticket_option': True,
            'titulo_sugerido': titulo
        })

    return jsonify({
        'response': 'Lo siento, no tengo la información suficiente para eso.',
        'suggestions': [],
        'ticket_option': False
    })


@tchat_bp.route('/crear_ticket', methods=['POST'])
def crear_ticket():
    data = request.get_json()
    nombre = data.get('nombre')
    titulo = data.get('titulo')
    descripcion = data.get('descripcion')

    if not all([nombre, titulo, descripcion]):
        return jsonify({"status": 400, "msg": "Faltan campos obligatorios."})

    # Load encoded credentials from environment variables or set them here
    ENCODED_USER = os.environ.get('ENCODED_USER', '')
    ENCODED_PASS = os.environ.get('ENCODED_PASS', '')
    if not ENCODED_USER or not ENCODED_PASS:
        return jsonify({"status": 500, "msg": "Credenciales codificadas no configuradas."})

    username = base64.b64decode(ENCODED_USER).decode()
    password = base64.b64decode(ENCODED_PASS).decode()
    categoria = "Chatbot ticket"

    token = login_api(username, password)
    if not token:
        return jsonify({"status": 401, "msg": "Credenciales inválidas o error de red."})

    status, msg = enviar_ticket(nombre, titulo, descripcion, categoria, token)
    return jsonify({"status": status, "msg": msg})




#Solo admin -------------------------------------------------------------------------------------


@tchat_bp.route('/upload_csv', methods=['POST'])
def upload_csv():
    global faq_loader
    file = request.files.get('file')
    if not file or not file.filename.endswith('.csv'):
        return "Archivo inválido. Debe ser .csv", 400

    upload_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'data')
    os.makedirs(upload_dir, exist_ok=True)
    path = os.path.join(upload_dir, file.filename)
    file.save(path)
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
    if not faq_loader:
        return jsonify({'success': False, 'error': 'Servicio no inicializado.'}), 500

    data_dir = getattr(faq_loader, 'data_dir', None)
    if not data_dir or not os.path.isdir(data_dir):
        return jsonify({'success': False, 'error': 'Directorio no encontrado.'}), 400

    data = request.get_json(force=True, silent=True)
    if not data or 'filename' not in data:
        return jsonify({'success': False, 'error': 'Falta el nombre del archivo.'}), 400

    filename = data['filename']
    if '/' in filename or '\\' in filename or '..' in filename:
        return jsonify({'success': False, 'error': 'Nombre de archivo inválido.'}), 400
    if not filename.endswith('.csv'):
        return jsonify({'success': False, 'error': 'Archivo debe ser .csv'}), 400

    file_path = os.path.join(data_dir, filename)
    if not os.path.isfile(file_path):
        return jsonify({'success': False, 'error': 'Archivo no existe.'}), 404

    try:
        os.remove(file_path)
        faq_loader.reload()
        return jsonify({'success': True})
    except Exception as e:
        return jsonify({'success': False, 'error': str(e)}), 500




#  Leer Get al CSV

 # [pregunta]:          
 # [respuesta]:;
 # [pregunta]:          
 # [respuesta]:;

# Escribir  Post que reciba pregunta y respuesta y lo agregue al csv actual 

@tchat_bp.route('/get_faq_raw', methods=['GET'])
def get_faq_raw():
    csv_path = os.path.join(faq_loader.data_dir, 'faqs.csv')
    if not os.path.isfile(csv_path):
        return "No se encontró el archivo.", 404

    try:
        import pandas as pd
        df = pd.read_csv(csv_path)
        lines = []
        for _, row in df.iterrows():
            pregunta = row['pregunta'].strip()
            respuesta = row['respuesta'].strip()
            lines.append(f"Pregunta: {pregunta}\nRespuesta: {respuesta};\n")
        return "\n".join(lines), 200, {'Content-Type': 'text/plain; charset=utf-8'}
    except Exception as e:
        return f"Error al leer el CSV: {str(e)}", 500



@tchat_bp.route('/add_faq', methods=['POST'])
def add_faq():
    global faq_loader, matcher

    data = request.get_json()
    pregunta = data.get('pregunta', '').strip()
    respuesta = data.get('respuesta', '').strip()

    if not pregunta or not respuesta:
        return jsonify({'success': False, 'error': 'Faltan pregunta o respuesta.'}), 400

    try:
        import pandas as pd
        csv_path = os.path.join(faq_loader.data_dir, 'faqs.csv')
        df = pd.DataFrame([{'pregunta': pregunta, 'respuesta': respuesta}])

        # Agrega la fila al final
        if not os.path.isfile(csv_path):
            df.to_csv(csv_path, index=False)
        else:
            df.to_csv(csv_path, mode='a', header=False, index=False)

        # Recargar en caliente
        faq_loader.reload()
        matcher = EmbeddingMatcher(faq_loader.questions, faq_loader.get_answer)

        return jsonify({'success': True})
    except Exception as e:
        return jsonify({'success': False, 'error': str(e)}), 500


