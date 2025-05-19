import json
import boto3
from boto3.dynamodb.conditions import Attr
from datetime import datetime, timedelta
from decimal import Decimal

# Initialize DynamoDB resource and target table
dynamodb = boto3.resource('dynamodb')
table = dynamodb.Table('WaterLevelTable')

# Function to convert Decimal types to int/float for JSON serialization
def decimal_default(obj):
    if isinstance(obj, Decimal):
        if obj % 1 == 0:
            return int(obj)
        else:
            return float(obj)
    raise TypeError

# Main Lambda handler function
def lambda_handler(event, context):
    try:
        # Get query parameters from URL
        params = event.get('queryStringParameters') or {}
        device_id = int(params.get('device_id'))  # Required: device ID
        time_range = params.get('range', 'last_day')  # Optional: time range

        # Get current UTC time
        now = datetime.utcnow()

        # Calculate start time based on requested range
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
            # Return error for invalid time range parameter
            return {
                'statusCode': 400,
                'body': json.dumps({'error': 'Invalid range parameter'})
            }

        # Convert start time to ISO 8601 string for comparison
        start_time_str = start_time.isoformat()

        # Scan DynamoDB for items matching device_id and newer than start time
        response = table.scan(
            FilterExpression=Attr('device_id').eq(device_id) & Attr('timestamp').gte(start_time_str)
        )

        # Get the matched items
        items = response.get('Items', [])

        # Return results with proper Decimal-to-JSON handling
        return {
            'statusCode': 200,
            'body': json.dumps({'data': items}, default=decimal_default)
        }

    except Exception as e:
        # Catch and return any error that occurs
        return {
            'statusCode': 500,
            'body': json.dumps({'error': str(e)})
        }
