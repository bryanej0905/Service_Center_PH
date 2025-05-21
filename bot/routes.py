# routes.py
from flask import Blueprint, request, jsonify, render_template
from utils import normalize_text

chat_bp = Blueprint('chat', __name__)
# Estos objetos los creamos en app.py y los importamos aquí
faq_loader = None  
matcher = None    

@chat_bp.route('/')
def index():
    return render_template('index.html')

@chat_bp.route('/chat', methods=['POST'])
def chat():
    raw = request.json.get('message', '')
    q_norm = normalize_text(raw)
    if not q_norm:
        return jsonify({'response': 'Por favor, escribe una pregunta válida.', 'suggestions': []})

    answer, suggestions = matcher.find(q_norm)
    if answer:
        return jsonify({'response': answer, 'suggestions': suggestions})
    else:
        return jsonify({'response': 'Lo siento, no tengo una respuesta para eso.', 'suggestions': []})
