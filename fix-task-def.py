import json
import sys

# Leer el JSON actual
with open('task-definition-frontend-current.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

# Eliminar campos no permitidos
fields_to_remove = ['taskDefinitionArn', 'revision', 'status', 'requiresAttributes', 
                    'placementConstraints', 'compatibilities', 'registeredAt', 
                    'registeredBy', 'deregisteredAt']
for field in fields_to_remove:
    data.pop(field, None)

# Remover cpu del container
if 'containerDefinitions' in data and len(data['containerDefinitions']) > 0:
    data['containerDefinitions'][0].pop('cpu', None)
    
    # Corregir variable de entorno
    if 'environment' in data['containerDefinitions'][0]:
        for env in data['containerDefinitions'][0]['environment']:
            if env['name'] == 'NEXT_PUBLIC_API_URL':
                env['value'] = 'https://api.zimmzimmgames.com'
                print(f"Variable corregida: {env['name']} = {env['value']}")

# Guardar JSON limpio
with open('task-definition-frontend-new.json', 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2, ensure_ascii=False)

print("JSON generado correctamente")
