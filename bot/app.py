# app.py
import os
from flask import Flask
from faq_loader import FAQLoader
from matcher import Matcher
import routes

def create_app():
    app = Flask(__name__)

    # 1) Calcula la ruta absoluta al CSV dentro de bot/data/faq.csv
    base_dir = os.path.dirname(os.path.abspath(__file__))  # .../Service_Center_PH/bot
    csv_path = os.path.join(base_dir, 'data', 'faq.csv')

    # 2) Inicializa el loader con la ruta correcta
    loader = FAQLoader(csv_path)

    # 3) Crea el matcher e inyecta en routes
    matcher_inst = Matcher(loader.questions, loader.get_answer)
    routes.faq_loader = loader
    routes.matcher    = matcher_inst

    # 4) Registra el blueprint
    app.register_blueprint(routes.chat_bp)
    return app

if __name__ == '__main__':
    app = create_app()
    app.run(debug=True)
