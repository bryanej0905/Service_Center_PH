import os
import pandas as pd
from utils import normalize_text

class FAQLoader:
    def __init__(self, csv_path: str = None):
        """
        Inicializa el loader de FAQs.
        Si no se proporciona csv_path, busca en \"<directorio_actual>/data/faq.csv\".
        """
        # Calcular ruta al CSV
        if csv_path:
            path = csv_path
        else:
            base_dir = os.path.dirname(os.path.abspath(__file__))
            path = os.path.join(base_dir, 'data', 'faq.csv')

        # Cargar DataFrame
        df = pd.read_csv(path)

        # Normalizar preguntas para las búsquedas
        df['pregunta_norm'] = df['pregunta'].apply(normalize_text)

        # Crear diccionario pregunta_norm -> respuesta
        self._q2a = dict(zip(df['pregunta_norm'], df['respuesta']))

    @property
    def questions(self):
        """Lista de preguntas normalizadas."""
        return list(self._q2a.keys())

    def get_answer(self, question_norm: str):
        """Devuelve la respuesta asociada a la pregunta normalizada."""
        return self._q2a.get(question_norm)
