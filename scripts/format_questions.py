import json

def escape_csharp_string(s):
    if s is None:
        return 'null'
    return '"' + s.replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n').replace('\r', '\\r') + '"'

def main():
    try:
        with open('/tmp/questions.json', 'r', encoding='utf-8') as f:
            questions = json.load(f)
    except Exception as e:
        print(f"Error loading JSON: {e}")
        return

    output = []
    for q in questions:
        category = escape_csharp_string(q.get('category'))
        sub_category = escape_csharp_string(q.get('subCategory'))
        text = escape_csharp_string(q.get('text'))
        image_url = escape_csharp_string(q.get('imageUrl'))
        option_a = escape_csharp_string(q.get('optionA'))
        option_b = escape_csharp_string(q.get('optionB'))
        option_c = escape_csharp_string(q.get('optionC'))
        option_d = escape_csharp_string(q.get('optionD'))
        correct_option = escape_csharp_string(q.get('correctOption'))
        explanation = escape_csharp_string(q.get('explanation'))

        line = f'            new() {{ Category={category}, SubCategory={sub_category}, Text={text}, ImageUrl={image_url}, OptionA={option_a}, OptionB={option_b}, OptionC={option_c}, OptionD={option_d}, CorrectOption={correct_option}, Explanation={explanation} }},'
        output.append(line)

    with open('/tmp/formatted_questions.txt', 'w', encoding='utf-8') as f:
        f.write('\n'.join(output))
    print("Formatting complete.")

if __name__ == "__main__":
    main()
