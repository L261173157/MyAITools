from fastapi import FastAPI
from functions.translator import TranslatorClass

app = FastAPI()
translator = TranslatorClass()

@app.get("/")
async def root():
    respon =  translator.translate("Hello,miss zhang", "chinese")
    return respon