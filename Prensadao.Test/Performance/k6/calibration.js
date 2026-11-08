import { setupOrderJourney, executeOrderJourney } from './order-journey.js';

const stages = [
  { name: 'calibration_01_rps', rate: 1, startTime: '0s' },
  { name: 'calibration_05_rps', rate: 5, startTime: '75s' },
  { name: 'calibration_10_rps', rate: 10, startTime: '150s' },
  { name: 'calibration_20_rps', rate: 20, startTime: '225s' },
];

function scenario(stage) {
  return {
    executor: 'constant-arrival-rate',
    exec: 'runOrderJourney',
    rate: stage.rate,
    timeUnit: '1s',
    duration: '60s',
    preAllocatedVUs: Math.max(10, stage.rate * 2),
    maxVUs: Math.max(20, stage.rate * 12),
    startTime: stage.startTime,
    gracefulStop: '10s',
  };
}

function metricSelector(metric, stage) {
  return `${metric}{scenario:${stage.name}}`;
}

const thresholds = {};
for (const stage of stages) {
  thresholds[metricSelector('checks', stage)] = ['rate>=0'];
  thresholds[metricSelector('http_req_failed', stage)] = ['rate>=0'];
  thresholds[metricSelector('dropped_iterations', stage)] = ['count>=0'];
  thresholds[metricSelector('iterations', stage)] = ['count>=0'];
  thresholds[metricSelector('order_create_duration', stage)] = ['p(95)>=0'];
  thresholds[metricSelector('order_to_preparing_duration', stage)] = ['p(95)>=0'];
}

export const options = {
  scenarios: Object.fromEntries(stages.map((stage) => [stage.name, scenario(stage)])),
  thresholds,
};

export function setup() {
  return setupOrderJourney();
}

export function runOrderJourney(data) {
  executeOrderJourney(data);
}

function value(data, metric, stage, field, fallback = 0) {
  return data.metrics[metricSelector(metric, stage)]?.values?.[field] ?? fallback;
}

function stageResult(data, stage) {
  const checksRate = value(data, 'checks', stage, 'rate');
  const httpFailureRate = value(data, 'http_req_failed', stage, 'rate');
  const droppedIterations = value(data, 'dropped_iterations', stage, 'count');
  const iterations = value(data, 'iterations', stage, 'count');
  const stable = checksRate === 1 && httpFailureRate < 0.01 && droppedIterations === 0;

  return {
    requestedRps: stage.rate,
    durationSeconds: 60,
    stable,
    iterations,
    droppedIterations,
    checksRate,
    httpFailureRate,
    orderCreateP95Ms: value(data, 'order_create_duration', stage, 'p(95)', null),
    orderToPreparingP95Ms: value(data, 'order_to_preparing_duration', stage, 'p(95)', null),
  };
}

export function handleSummary(data) {
  const results = stages.map((stage) => stageResult(data, stage));
  const stableStages = results.filter((stage) => stage.stable);
  const highestStableRps = stableStages.length === 0
    ? null
    : Math.max(...stableStages.map((stage) => stage.requestedRps));
  const firstUnstableStage = results.find((stage) => !stage.stable) ?? null;
  const proposedRegression = highestStableRps === null
    ? null
    : {
        rateRps: highestStableRps * 0.5,
        durationSeconds: 120,
      };
  const report = {
    schemaVersion: 1,
    profile: 'calibration',
    capturedAt: new Date().toISOString(),
    stages: results,
    highestStableRps,
    firstUnstableRps: firstUnstableStage?.requestedRps ?? null,
    proposedRegression,
  };
  const stableDescription = highestStableRps === null
    ? 'nenhuma etapa estavel'
    : `maior etapa estavel: ${highestStableRps} pedido(s)/s`;

  return {
    stdout: `Calibracao k6 concluida; ${stableDescription}.\n`,
    '/results/calibration-summary.json': JSON.stringify(report, null, 2),
  };
}
