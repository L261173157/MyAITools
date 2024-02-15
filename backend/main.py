from fastapi import FastAPI

app = FastAPI()


@app.get("/")
async def root():
    return {"message": "您好，linux中国！"}