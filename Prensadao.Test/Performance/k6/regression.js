import { executeOrderJourney, setupOrderJourney } from './order-journey.js';

const rateRps = Number.parseInt(__ENV.RATE_RPS, 10);
const durationSeconds = Number.parseInt(__ENV.DURATION_SECONDS, 10);
const baselineOrderCreateP95Ms = Number.parseFloat(__ENV.BASELINE_ORDER_CREATE_P95_MS);
const baselineOrderToPreparingP95Ms = Number.parseFloat(__ENV.BASELINE_ORDER_TO_PREPARING_P95_MS);
const maxOrderCreateP95Ms = Number.parseFloat(__ENV.MAX_ORDER_CREATE_P95_MS);
const maxOrderToPreparingP95Ms = Number.parseFloat(__ENV.MAX_ORDER_TO_PREPARING_P95_MS);

for (const [name, value] of Object.entries({
  RATE_RPS: rateRps,
  DURATION_SECONDS: durationSeconds,
  BASELINE_ORDER_CREATE_P95_MS: baselineOrderCreateP95Ms,
  BASELINE_ORDER_TO_PREPARING_P95_MS: baselineOrderToPreparingP95Ms,
  MAX_ORDER_CREATE_P95_MS: maxOrderCreateP95Ms,
  MAX_ORDER_TO_PREPARING_P95_MS: maxOrderToPreparingP95Ms,
})) {
  if (!Number.isFinite(value) || value <= 0) {
    throw new Error(`${name} must be a positive number.`);
  }
}

export const options = {
  scenarios: {
    regression: {
      executor: 'constant-arrival-rate',
      rate: rateRps,
      timeUnit: '1s',
      duration: `${durationSeconds}s`,
      preAllocatedVUs: Math.max(10, rateRps * 2),
      maxVUs: Math.max(20, rateRps * 12),
      gracefulStop: '10s',
    },
  },
  thresholds: {
    checks: ['rate==1'],
    http_req_failed: ['rate<0.01'],
    dropped_iterations: ['count==0'],
    iterations: [`count>=${rateRps * durationSeconds}`],
    order_create_duration: [`p(95)<=${maxOrderCreateP95Ms}`],
    order_to_preparing_duration: [`p(95)<=${maxOrderToPreparingP95Ms}`],
  },
};

export function setup() {
  return setupOrderJourney();
}

export default function (data) {
  executeOrderJourney(data);
}

export function handleSummary(data) {
  const orderCreateP95Ms = data.metrics.order_create_duration?.values?.['p(95)'] ?? null;
  const orderToPreparingP95Ms = data.metrics.order_to_preparing_duration?.values?.['p(95)'] ?? null;
  const report = {
    schemaVersion: 1,
    profile: {
      rateRps,
      durationSeconds,
    },
    executedAt: new Date().toISOString(),
    functional: {
      iterations: data.metrics.iterations?.values?.count ?? 0,
      droppedIterations: data.metrics.dropped_iterations?.values?.count ?? 0,
      checksRate: data.metrics.checks?.values?.rate ?? 0,
      httpFailureRate: data.metrics.http_req_failed?.values?.rate ?? 0,
    },
    latency: {
      orderCreate: {
        currentP95Ms: orderCreateP95Ms,
        baselineP95Ms: baselineOrderCreateP95Ms,
        maximumP95Ms: maxOrderCreateP95Ms,
        passed: orderCreateP95Ms !== null && orderCreateP95Ms <= maxOrderCreateP95Ms,
      },
      orderToPreparing: {
        currentP95Ms: orderToPreparingP95Ms,
        baselineP95Ms: baselineOrderToPreparingP95Ms,
        maximumP95Ms: maxOrderToPreparingP95Ms,
        passed: orderToPreparingP95Ms !== null && orderToPreparingP95Ms <= maxOrderToPreparingP95Ms,
      },
    },
  };

  return {
    stdout: `Regressao k6 concluida: ${report.functional.iterations} pedidos executados.\n`,
    '/results/regression-summary.json': JSON.stringify(report, null, 2),
  };
}
