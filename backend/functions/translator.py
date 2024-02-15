from dotenv import load_dotenv
import os
from openai import OpenAI


class TranslatorClass:

    def __init__(self):
        load_dotenv()
        self.client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))
        self.system_content = (
            "you are a good translator,can translate any language to any language"
        )

    def translate(self, text: str, dest_language: str):
        completion = self.client.chat.completions.create(
            model="gpt-3.5-turbo",
            messages=[
                {"role": "system", "content": self.system_content},
                {"role": "user", "content": text + " to " + dest_language},
            ],
        )
        return completion.choices[0].message.content
