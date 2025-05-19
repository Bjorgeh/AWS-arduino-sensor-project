import json
import boto3
from datetime import datetime

# Initialize DynamoDB resource and specify the target table
dynamodb = boto3.resource('dynamodb')
table = dynamodb.Table('WaterLevelTable')

# Main AWS Lambda handler function
def lambda_handler(event, context):
    try:
        # Parse JSON body from the HTTP POST request
        data = json.loads(event['body'])

        # Extract required fields from the input
        device_id = int(data['device_id'])        # Device identifier
        water_level = int(data['water_level'])    # Water level measurement

        # Generate current timestamp in ISO 8601 format
        timestamp = datetime.utcnow().isoformat()

        # Construct the item to insert into DynamoDB
        item = {
            'device_id': device_id,
            'timestamp': timestamp,
            'water_level': water_level
        }

        # Insert the item into the DynamoDB table
        table.put_item(Item=item)

        # Return success response
        return {
            'statusCode': 200,
            'body': json.dumps({'message': 'Data saved successfully!'})
        }

    except Exception as e:
        # Return error message if something goes wrong
        return {
            'statusCode': 500,
            'body': json.dumps({'error': str(e)})
        }
