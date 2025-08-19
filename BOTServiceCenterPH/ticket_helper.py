import spacy
import requests
from better_profanity import Profanity
from flask import session

# Cargar modelo de spaCy para español
nlp = spacy.load("es_core_news_sm")

# Configurar censor en español
profanity = Profanity()
profanity.load_censor_words_from_file("spanish_badwords.txt")

def store_user_input(input_text):
    session.setdefault('recent_inputs', [])
    session['recent_inputs'].append(input_text)
    if len(session['recent_inputs']) > 3:
        session['recent_inputs'].pop(0)

def contains_bad_words(texts):
    return any(profanity.contains_profanity(t) for t in texts)

def extract_nouns(texts):
    nouns = []
    for text in texts:
        doc = nlp(text)
        nouns.extend([token.text.capitalize() for token in doc if token.pos_ == "NOUN"])
    return list(dict.fromkeys(nouns))  # Eliminar duplicados, conservar orden

def generar_titulo_ticket(nouns):
    return ", ".join(nouns)

def enviar_ticket(titulo, descripcion, categoria):
    payload = {
        "titulo": titulo,
        "descripcion": descripcion,
        "categoria": categoria
    }
    response = requests.post("https://api.servicecenterph.com/api/Tickets", json=payload)
    return response.status_code, response.text
