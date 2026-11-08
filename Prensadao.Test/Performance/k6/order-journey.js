import http from 'k6/http';
import { check, fail, sleep } from 'k6';
import { Trend } from 'k6/metrics';

const performanceBaseUrl = __ENV.BASE_URL || 'http://api:8080';

export const orderCreateDuration = new Trend('order_create_duration', true);
export const orderToPreparingDuration = new Trend('order_to_preparing_duration', true);

const jsonRequest = {
  headers: {
    'Content-Type': 'application/json',
  },
};

function post(path, body, operation) {
  return http.post(`${performanceBaseUrl}${path}`, JSON.stringify(body), {
    ...jsonRequest,
    tags: { operation },
  });
}

function responseData(response, operation) {
  const responseAccepted = check(response, {
    [`${operation} returns HTTP 200`]: (result) => result.status === 200,
  });

  if (!responseAccepted) {
    fail(`${operation} failed with HTTP ${response.status}: ${response.body}`);
  }

  let body;
  try {
    body = response.json();
  } catch (error) {
    fail(`${operation} returned invalid JSON: ${error.message}`);
  }

  if (!body || !Number.isInteger(body.data) || body.data <= 0) {
    fail(`${operation} did not return a valid data identifier.`);
  }

  return body.data;
}

export function setupOrderJourney() {
  const suffix = `${Date.now()}`;
  const customerResponse = post('/api/Customer/Post', {
    name: 'Cliente k6',
    phone: suffix.slice(-11).padStart(11, '9'),
    street: 'Rua de performance',
    district: 'Centro',
    number: '100',
    city: 'Criciuma',
    referencePoint: 'Ambiente descartavel',
    cep: 88800000,
  }, 'create_customer');

  const productResponse = post('/api/Product/Post', {
    name: `Produto k6 ${suffix}`,
    enabled: true,
    price: 25.90,
    description: 'Produto criado pelo teste de performance do k6',
  }, 'create_product');

  return {
    customerId: responseData(customerResponse, 'create customer'),
    productId: responseData(productResponse, 'create product'),
  };
}

export function executeOrderJourney(data) {
  const startedAt = Date.now();
  const createResponse = post('/api/Order/Post', {
    isDelivery: false,
    notes: 'Teste de performance k6',
    customerId: data.customerId,
    orderItems: [
      {
        quantity: 1,
        productId: data.productId,
      },
    ],
  }, 'create_order');

  orderCreateDuration.add(createResponse.timings.duration);
  const orderId = responseData(createResponse, 'create order');
  const deadline = Date.now() + 10000;
  let finalStatus = null;

  while (Date.now() < deadline) {
    const statusResponse = http.get(`${performanceBaseUrl}/api/Order/GetById?id=${orderId}`, {
      tags: { operation: 'get_order_status' },
    });

    if (statusResponse.status === 200) {
      finalStatus = statusResponse.json('status');
      if (finalStatus === 'Em preparação') {
        orderToPreparingDuration.add(Date.now() - startedAt);
        break;
      }
    }

    sleep(0.1);
  }

  check(finalStatus, {
    'order reaches Em preparação within 10 seconds': (status) => status === 'Em preparação',
  });
}
