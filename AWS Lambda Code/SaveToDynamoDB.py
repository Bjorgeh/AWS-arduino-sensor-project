import json
import boto3
from datetime import datetime

dynamodb = boto3.resource('dynamodb')
table = dynamodb.Table('WaterLevelTable')

def lambda_handler(event, context):
    try:
        data = json.loads(event['body'])
        device_id = int(data['device_id'])
        water_level = int(data['water_level'])

        timestamp = datetime.utcnow().isoformat()

        item = {
            'device_id': device_id,
            'timestamp': timestamp,
            'water_level': water_level
        }

        table.put_item(Item=item)

        return {
            'statusCode': 200,
            'body': json.dumps({'message': 'Data saved successfully!'})
        }

    except Exception as e:
        return {
            'statusCode': 500,
            'body': json.dumps({'error': str(e)})
        }
