from flask import Flask, request, jsonify, render_template
import pandas as pd
from difflib import get_close_matches

app = Flask(__name__)

# Cargar preguntas y respuestas desde el CSV
data = pd.read_csv('data/faq.csv')
faq_dict = dict(zip(data['pregunta'], data['respuesta']))
preguntas = list(faq_dict.keys())

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/chat', methods=['POST'])
def chat():
    user_question = request.json.get('message', '').strip().lower()

    if not user_question:
        return jsonify({'response': 'Por favor, escribe una pregunta válida.'})

    # Buscar coincidencias exactas
    for pregunta in preguntas:
        if user_question == pregunta.lower():
            sugerencias = get_close_matches(user_question, preguntas, n=3, cutoff=0.3)
            sugerencias = [s for s in sugerencias if s.lower() != pregunta.lower()]
            return jsonify({
                'response': faq_dict[pregunta],
                'suggestions': sugerencias
            })

    # Buscar coincidencias difusas
    matches = get_close_matches(user_question, preguntas, n=3, cutoff=0.4)

    if matches:
        respuesta = faq_dict[matches[0]]
        sugerencias = matches[1:]  # Las siguientes 2 sugerencias
        return jsonify({
            'response': respuesta,
            'suggestions': sugerencias
        })
    else:
        return jsonify({
            'response': 'Lo siento, no tengo una respuesta para eso.',
            'suggestions': []
        })

if __name__ == '__main__':
    app.run(debug=True)
