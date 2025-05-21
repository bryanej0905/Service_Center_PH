import numpy as np
from sentence_transformers import SentenceTransformer
from sklearn.metrics.pairwise import cosine_similarity

class EmbeddingMatcher:
    def __init__(self,
                 questions,
                 lookup_fn,
                 model_name: str = 'all-MiniLM-L6-v2',
                 top_k: int = 3):
        """
        Matcher semántico basado en embeddings de Sentence-Transformers.

        questions: lista de strings normalizados.
        lookup_fn: función(question_norm) -> respuesta.
        model_name: nombre del modelo S-BERT a usar.
        top_k: número de resultados (1 respuesta + top_k-1 sugerencias).
        """
        self.questions = questions
        self.lookup = lookup_fn
        self.top_k = top_k

        # Carga del modelo y cálculo de embeddings de todas las preguntas
        self.model = SentenceTransformer(model_name)
        self.q_embeddings = self.model.encode(
            questions,
            convert_to_numpy=True,
            show_progress_bar=False
        )

    def find(self, q_norm: str):
        """
        Dada una consulta normalizada, devuelve (respuesta, [sugerencias]).
        """
        # Generar embedding de la consulta
        q_emb = self.model.encode([q_norm], convert_to_numpy=True)
        # Calcular similitud coseno contra todas las preguntas
        sims = cosine_similarity(q_emb, self.q_embeddings).ravel()
        # Ordenar índices por similitud descendente
        idx_sorted = np.argsort(-sims)
        # Tomar los top_k primeros
        top_idxs = idx_sorted[:self.top_k]

        # Primera mejor coincidencia como respuesta
        best_q = self.questions[top_idxs[0]]
        resp = self.lookup(best_q)
        # Las siguientes top_k-1 como sugerencias
        suggestions = [self.questions[i] for i in top_idxs[1:]]
        return resp, suggestions
