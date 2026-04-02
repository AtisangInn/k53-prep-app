import urllib.request
import urllib.parse
import hashlib

# 1. This is the exact data PayFast sends to our Webhook when a sandbox payment succeeds
m_payment_id = input("Enter the m_payment_id from your database (e.g. from StudentPayments table): ").strip()

data = {
    'm_payment_id': m_payment_id,
    'pf_payment_id': '19069391',
    'payment_status': 'COMPLETE',
    'item_name': 'K53 Academy Premium',
    'item_description': '',
    'amount_gross': '79.00',
    'amount_fee': '-2.60',
    'amount_net': '76.40',
    'custom_str1': '',
    'custom_str2': '',
    'custom_str3': '',
    'custom_str4': '',
    'custom_str5': '',
    'custom_int1': '',
    'custom_int2': '',
    'custom_int3': '',
    'custom_int4': '',
    'custom_int5': '',
    'name_first': 'Test',
    'name_last': 'User',
    'email_address': 'test@user.com',
    'merchant_id': '10000100'
}

# 2. Generate the exactly matching signature
param_string = "&".join(f"{k}={urllib.parse.quote_plus(v)}" for k, v in data.items())
signature = hashlib.md5(param_string.encode('utf-8')).hexdigest()
data['signature'] = signature

# 3. Send the simulated webhook request
url = 'http://localhost:5000/api/payments/notify'
encoded_data = urllib.parse.urlencode(data).encode('utf-8')
req = urllib.request.Request(url, data=encoded_data, method='POST')
req.add_header('Content-Type', 'application/x-www-form-urlencoded')

print(f"Sending simulated PayFast ITN webhook to {url}...")
try:
    with urllib.request.urlopen(req) as response:
        print(f"Success! Server responded with HTTP {response.status}")
except Exception as e:
    print(f"Error sending webhook: {e}")

print("\nIf successful, the backend should now have upgraded the student to Premium!")
