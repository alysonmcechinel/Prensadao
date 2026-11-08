import { executeOrderJourney, setupOrderJourney } from './order-journey.js';

const rateRps = Number.parseInt(__ENV.RATE_RPS, 10);
const durationSeconds = Number.parseInt(__ENV.DURATION_SECONDS, 10);

if (!Number.isInteger(rateRps) || rateRps <= 0) {
  throw new Error('RATE_RPS must be a positive integer.');
}

if (!Number.isInteger(durationSeconds) || durationSeconds <= 0) {
  throw new Error('DURATION_SECONDS must be a positive integer.');
}

export const options = {
  scenarios: {
    baseline_candidate: {
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
  },
};

export function setup() {
  return setupOrderJourney();
}

export default function (data) {
  executeOrderJourney(data);
}

export function handleSummary(data) {
  const report = {
    schemaVersion: 1,
    profile: {
      rateRps,
      durationSeconds,
    },
    capturedAt: new Date().toISOString(),
    iterations: data.metrics.iterations?.values?.count ?? 0,
    droppedIterations: data.metrics.dropped_iterations?.values?.count ?? 0,
    checksRate: data.metrics.checks?.values?.rate ?? 0,
    httpFailureRate: data.metrics.http_req_failed?.values?.rate ?? 0,
    orderCreateP95Ms: data.metrics.order_create_duration?.values?.['p(95)'] ?? null,
    orderToPreparingP95Ms: data.metrics.order_to_preparing_duration?.values?.['p(95)'] ?? null,
  };

  return {
    stdout: `Candidato a baseline concluido: ${report.iterations} pedidos executados.\n`,
    '/results/baseline-candidate-summary.json': JSON.stringify(report, null, 2),
  };
}
