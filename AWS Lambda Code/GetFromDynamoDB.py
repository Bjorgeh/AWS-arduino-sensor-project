import json
import boto3
from boto3.dynamodb.conditions import Attr
from datetime import datetime, timedelta
from decimal import Decimal

dynamodb = boto3.resource('dynamodb')
table = dynamodb.Table('WaterLevelTable')

def decimal_default(obj):
    if isinstance(obj, Decimal):
        if obj % 1 == 0:
            return int(obj)
        else:
            return float(obj)
    raise TypeError

def lambda_handler(event, context):
    try:
        params = event.get('queryStringParameters') or {}
        device_id = int(params.get('device_id'))
        time_range = params.get('range', 'last_day')

        now = datetime.utcnow()
        if time_range == 'last_hour':
            start_time = now - timedelta(hours=1)
        elif time_range == 'last_6_hours':
            start_time = now - timedelta(hours=6)
        elif time_range == 'last_12_hours':
            start_time = now - timedelta(hours=12)
        elif time_range == 'last_day':
            start_time = now - timedelta(days=1)
        elif time_range == 'last_week':
            start_time = now - timedelta(weeks=1)
        elif time_range == 'last_month':
            start_time = now - timedelta(days=30)
        else:
            return {
                'statusCode': 400,
                'body': json.dumps({'error': 'Invalid range parameter'})
            }

        start_time_str = start_time.isoformat()

        response = table.scan(
            FilterExpression=Attr('device_id').eq(device_id) & Attr('timestamp').gte(start_time_str)
        )

        items = response.get('Items', [])
        return {
            'statusCode': 200,
            'body': json.dumps({'data': items}, default=decimal_default)
        }

    except Exception as e:
        return {
            'statusCode': 500,
            'body': json.dumps({'error': str(e)})
        }
