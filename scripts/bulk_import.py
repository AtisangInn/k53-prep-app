import json
import requests

API_BASE = 'https://k53-prep-app-production.up.railway.app/api'
ADMIN_CODE = 'goduadmink53'

def main():
    print(f"=== K53 Bulk Import ===")
    print(f"Target: {API_BASE}")
    
    try:
        with open('new_questions.json', 'r', encoding='utf-8') as f:
            questions = json.load(f)
    except FileNotFoundError:
        print("Error: new_questions.json not found.")
        return

    print(f"Ready to import {len(questions)} questions.")
    
    confirm = input("Are you sure you want to replace all existing questions on production? (y/n): ")
    if confirm.lower() != 'y':
        print("Aborted.")
        return

    headers = {
        'Content-Type': 'application/json',
        'X-Admin-Code': ADMIN_CODE
    }

    try:
        response = requests.post(f"{API_BASE}/questions/bulk", headers=headers, json=questions)
        if response.ok:
            data = response.json()
            print(f"\nSUCCESS!")
            print(f"Deleted: {data['deletedCount']}")
            print(f"Imported: {data['importedCount']}")
            print(f"Message: {data['message']}")
        else:
            print(f"\nFAILED: {response.status_code}")
            print(response.text)
    except Exception as e:
        print(f"\nERROR: {e}")

if __name__ == "__main__":
    main()
