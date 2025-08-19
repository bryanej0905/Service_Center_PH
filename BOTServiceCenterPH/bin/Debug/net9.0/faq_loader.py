import os
import pandas as pd
from utils import normalize_text

class FAQLoader:
    def __init__(self, data_dir: str = None):
        """
        Carga todos los CSVs del directorio especificado (por defecto: carpeta /data).
        Combina todas las preguntas/respuestas en un diccionario interno.
        """
        base_dir = os.path.dirname(os.path.abspath(__file__))
        self.data_dir = data_dir or os.path.join(base_dir, 'data')

        if not os.path.isdir(self.data_dir):
            raise NotADirectoryError(f"{self.data_dir} no es un directorio válido")

        self._load_all_csvs()

    def _load_all_csvs(self):
        """Carga y fusiona todos los CSVs válidos en el directorio."""
        df_list = []
        for file in os.listdir(self.data_dir):
            if file.endswith('.csv'):
                path = os.path.join(self.data_dir, file)
                try:
                    df = pd.read_csv(path)
                    if {'pregunta', 'respuesta'}.issubset(df.columns):
                        df['pregunta_norm'] = df['pregunta'].apply(normalize_text)
                        df_list.append(df[['pregunta_norm', 'respuesta']])
                except Exception as e:
                    print(f"Error cargando {file}: {e}")

        if df_list:
            merged_df = pd.concat(df_list).drop_duplicates(subset='pregunta_norm')
            self._q2a = dict(zip(merged_df['pregunta_norm'], merged_df['respuesta']))
        else:
            self._q2a = {}

    def reload(self):
        """Permite recargar los CSVs, útil después de subir uno nuevo."""
        self._load_all_csvs()

    @property
    def questions(self):
        return list(self._q2a.keys())

    def get_answer(self, question_norm: str):
        return self._q2a.get(question_norm)
