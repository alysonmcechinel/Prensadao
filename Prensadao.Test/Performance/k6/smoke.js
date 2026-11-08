import { setupOrderJourney, executeOrderJourney } from './order-journey.js';

export const options = {
  scenarios: {
    smoke: {
      executor: 'shared-iterations',
      vus: 1,
      iterations: 5,
      maxDuration: '1m',
    },
  },
  thresholds: {
    checks: ['rate==1'],
    http_req_failed: ['rate<0.01'],
    iterations: ['count==5'],
    dropped_iterations: ['count==0'],
  },
};

export function setup() {
  return setupOrderJourney();
}

export default function (data) {
  executeOrderJourney(data);
}

export function handleSummary(data) {
  const iterationCount = data.metrics.iterations?.values?.count ?? 0;

  return {
    stdout: `Smoke k6 concluido: ${iterationCount} pedidos executados.\n`,
    '/results/smoke-summary.json': JSON.stringify(data, null, 2),
  };
}
