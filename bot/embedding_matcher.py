import numpy as np
from sklearn.metrics.pairwise import cosine_similarity

class EmbeddingMatcher:
    def __init__(
        self,
        questions,
        lookup_fn,
        model_name: str = 'all-MiniLM-L6-v2',
        top_k: int = 3
    ):
        """
        Matcher semántico basado en embeddings de Sentence-Transformers,
        pero con carga lazy del modelo para acelerar el arranque.
        """
        self.questions    = questions
        self.lookup       = lookup_fn
        self.model_name   = model_name
        self.top_k        = top_k
        self.model        = None
        self.q_embeddings = None

    def _load_model(self):
        # Import dinámico para que torch no se cargue al iniciar el servidor
        from sentence_transformers import SentenceTransformer
        self.model = SentenceTransformer(self.model_name)
        # Vectorizamos TODO el corpus DE PREGUNTAS
        self.q_embeddings = self.model.encode(
            self.questions,
            convert_to_numpy=True,
            show_progress_bar=False
        )

    def find(self, q_norm: str):
        if self.model is None:
            self._load_model()

        q_emb = self.model.encode([q_norm], convert_to_numpy=True)
        sims = cosine_similarity(q_emb, self.q_embeddings).ravel()
        idx_sorted = np.argsort(-sims)
        top_idxs = idx_sorted[:self.top_k]

        best_q = self.questions[top_idxs[0]]
        resp = self.lookup(best_q)
        suggestions = [self.questions[i] for i in top_idxs[1:]]
        score = float(sims[top_idxs[0]])

        #  Nueva lógica: forzamos precisión al filtrar respuestas poco confiables
        if score < 0.88:
            # Puedes devolver una respuesta genérica si la similitud es baja,
            # o dejar la respuesta pero influenciar el frontend (como haces ahora).
            resp = "Lo siento, no tengo información suficiente para eso"
            suggestions = []

        return resp, suggestions, score

