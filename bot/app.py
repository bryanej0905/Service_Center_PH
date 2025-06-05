import os
from flask import Flask
from faq_loader import FAQLoader
from embedding_matcher import EmbeddingMatcher
import routes

def create_app():
    app = Flask(__name__)

    # 1) Ruta absoluta al directorio 'data' (donde estarán los CSVs)
    base_dir = os.path.dirname(os.path.abspath(__file__))  # .../Service_Center_PH/bot
    data_dir = os.path.join(base_dir, 'data')              # aquí debe ir el directorio, NO un archivo

    # 2) Inicializa el loader con el directorio correcto
    loader = FAQLoader(data_dir)

    # 3) Crea el EmbeddingMatcher e inyecta en routes
    matcher_inst = EmbeddingMatcher(loader.questions, loader.get_answer)
    routes.faq_loader = loader
    routes.matcher = matcher_inst

    # 4) Registra el blueprint
    app.register_blueprint(routes.tchat_bp)
    return app

if __name__ == '__main__':
    app = create_app()
    app.run(debug=False)
