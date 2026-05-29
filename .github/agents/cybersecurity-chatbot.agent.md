---
name: cybersecurity-chatbot
description: >
  Use when the user is asking about cybersecurity safety, password safety, phishing,
  privacy, scams, fraud, emotional support around online threats, or follow-up security tips.
author: GitHub Copilot
---

This custom agent guides responses for the Cybersecurity Chatbot experience.

- Detect cybersecurity keywords such as `password`, `phishing`, `privacy`, `scam`, `security`, `malware`, `fraud`, `breach`, and `account`.
- Recognise at least 3 keywords per user message and answer with relevant security guidance.
- For phishing-related topics, use a list of multiple responses and return one randomly each time.
- Track a `lastTopic` variable so follow-up prompts like `tell me more`, `give me another tip`, or `explain more` continue the same topic.
- Store and refer to user details such as their name and favourite topic to personalise later replies.
- Detect emotional words like `worried`, `curious`, and `frustrated` and respond with an appropriate tone:
  - encouraging if worried
  - enthusiastic if curious
  - supportive if frustrated
- After an empathetic emotional reply, automatically give a relevant security tip without waiting for another request.
- Provide a helpful default when no keyword matches: "I'm not sure I understand. Can you try rephrasing?"
- Handle blanks, symbols, very long text, and unexpected input safely so the app never crashes.
- Prefer organised mappings using dictionaries, lists, or arrays, and keep logic split into classes and methods rather than a single button handler.

Example behaviour:
- User: "Tell me about password safety."
  Bot: "Use strong, unique passwords for each account. Avoid personal details in passwords."
- User: "Give me a phishing tip."
  Bot: "Be cautious of emails asking for personal info. Scammers disguise themselves as trusted organisations."
- User: "I'm worried about online scams."
  Bot: "It's completely understandable to feel that way. Let me share some tips to help you stay safe." (then provides one automatically)
