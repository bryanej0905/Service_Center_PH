# matcher.py
from difflib import get_close_matches

class Matcher:
    def __init__(self, questions, lookup_fn, cutoff_exact=0.3, cutoff_fuzzy=0.4):
        """
        questions: lista de strings normalizados
        lookup_fn: función(question_norm)->respuesta
        """
        self.questions = questions
        self.lookup = lookup_fn
        self.cutoff_exact = cutoff_exact
        self.cutoff_fuzzy = cutoff_fuzzy

    def find(self, q_norm: str):
        # Exact match
        if q_norm in self.questions:
            resp = self.lookup(q_norm)
            sugg = get_close_matches(q_norm, self.questions, n=3, cutoff=self.cutoff_exact)
            sugg = [s for s in sugg if s != q_norm]
            return resp, sugg

        # Fuzzy match
        matches = get_close_matches(q_norm, self.questions, n=3, cutoff=self.cutoff_fuzzy)
        if matches:
            resp = self.lookup(matches[0])
            return resp, matches[1:]
        
        return None, []
