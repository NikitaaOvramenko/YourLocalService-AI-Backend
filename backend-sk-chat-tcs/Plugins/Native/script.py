import sys
import requests
from io import BytesIO
import PIL.Image
from google import genai
from google.genai import types
import base64
import os

# Args from C#
imageUrl = sys.argv[1]   # Supabase public URL of input image
prompt = sys.argv[2]

# Download original image
response = requests.get(imageUrl)
img = PIL.Image.open(BytesIO(response.content))

# Gemini client
client = genai.Client(api_key=os.environ["GEMINI_APIKEY"])

# Generate new image
response = client.models.generate_content(
    model="gemini-2.0-flash-preview-image-generation",
    contents=[prompt, img],
    config=types.GenerateContentConfig(response_modalities=['TEXT', 'IMAGE'])
)

# Output the new image as Base64
for part in response.candidates[0].content.parts:
    if part.inline_data is not None:
        new_img = PIL.Image.open(BytesIO(part.inline_data.data))
        buffer = BytesIO()
        new_img.save(buffer, format="PNG")
        img_bytes = buffer.getvalue()
        base64_str = base64.b64encode(img_bytes).decode("utf-8")
        print(base64_str)  
