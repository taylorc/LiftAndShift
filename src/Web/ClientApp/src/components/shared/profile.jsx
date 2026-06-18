import { useState, useEffect, use } from 'react';
import { useNavigate } from 'react-router-dom';
import { OnboardingClient, SaveUserOnboardingCommand } from '../../web-api-client';
import { useAuth } from '../api-authorization/AuthContext';

const client = new OnboardingClient();

export function Profile({heading, subTitle}) {
  const { refreshOnboardingStatus } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isBegginner, setIsBeginner] = useState(false);

  const [form, setForm] = useState({
    preferredUnit: 'Lbs',
    bodyWeight: '',
    alternatingLift: 'PowerClean',
    squatStartingWeight: '',
    benchPressStartingWeight: '',
    overheadPressStartingWeight: '',
    deadliftStartingWeight: '',
    alternatingLiftStartingWeight: '',
  });

  useEffect(() => {
    client.getOnboarding().then(data => {
      if (data.isOnboarded) {
        setIsBeginner(true);
        setForm({
          preferredUnit: data.preferredUnit ?? 'Lbs',
          bodyWeight: data.bodyWeight?.toString() ?? '',
          alternatingLift: data.alternatingLift ?? 'PowerClean',
          squatStartingWeight: data.squatStartingWeight?.toString() ?? '',
          benchPressStartingWeight: data.benchPressStartingWeight?.toString() ?? '',
          overheadPressStartingWeight: data.overheadPressStartingWeight?.toString() ?? '',
          deadliftStartingWeight: data.deadliftStartingWeight?.toString() ?? '',
          alternatingLiftStartingWeight: data.alternatingLiftStartingWeight?.toString() ?? '',
        });
      }
    }).catch(() => {});
  }, []);

  const set = field => e => setForm(f => ({ ...f, [field]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);
    try {
      await client.saveOnboarding(new SaveUserOnboardingCommand({
        preferredUnit: form.preferredUnit,
        bodyWeight: parseFloat(form.bodyWeight),
        alternatingLift: form.alternatingLift,
        squatStartingWeight: parseFloat(form.squatStartingWeight),
        benchPressStartingWeight: parseFloat(form.benchPressStartingWeight),
        overheadPressStartingWeight: parseFloat(form.overheadPressStartingWeight),
        deadliftStartingWeight: parseFloat(form.deadliftStartingWeight),
        alternatingLiftStartingWeight: parseFloat(form.alternatingLiftStartingWeight),
      }));
      await refreshOnboardingStatus();
      navigate('/');
    } catch {
      setError('Failed to save. Please check your inputs and try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const unitLabel = form.preferredUnit === 'Lbs' ? 'lbs' : 'kg';
  const altLiftLabel = form.alternatingLift === 'PowerClean' ? 'Power Clean' : 'Pendlay Row';

  return (
    <article>
      <h2>{heading}</h2>
      <p>{subTitle}</p>

      {error && <p role="alert" style={{ color: 'var(--pico-color-red-500)' }}>{error}</p>}

      <form onSubmit={handleSubmit}>
        <fieldset>
          <legend><strong>Preferred Unit</strong></legend>
          <label>
            <input type="radio" name="unit" value="Lbs"
              checked={form.preferredUnit === 'Lbs'}
              onChange={set('preferredUnit')} />
            {' '}Lbs
          </label>
          <label>
            <input type="radio" name="unit" value="Kgs"
              checked={form.preferredUnit === 'Kgs'}
              onChange={set('preferredUnit')} />
            {' '}Kgs
          </label>
        </fieldset>

        <label htmlFor="bodyWeight">Body Weight ({unitLabel})</label>
        <input type="number" id="bodyWeight" min="0" step="0.1" required
          value={form.bodyWeight} onChange={set('bodyWeight')} />

        <fieldset hidden={!isBegginner}>
          <legend><strong>Alternating Lift (Workout B)</strong></legend>
          <label>
            <input type="radio" name="altLift" value="PowerClean"
              checked={form.alternatingLift === 'PowerClean'}
              onChange={set('alternatingLift')} />
            {' '}Power Clean
          </label>
          <label>
            <input type="radio" name="altLift" value="PendlayRow"
              checked={form.alternatingLift === 'PendlayRow'}
              onChange={set('alternatingLift')} />
            {' '}Pendlay Row
          </label>
        </fieldset>

        <h3>Starting Working Weights ({unitLabel})</h3>

        <label htmlFor="squat">Squat</label>
        <input type="number" id="squat" min="0" step="2.5" required
          value={form.squatStartingWeight} onChange={set('squatStartingWeight')} />

        <label htmlFor="bench">Bench Press</label>
        <input type="number" id="bench" min="0" step="2.5" required
          value={form.benchPressStartingWeight} onChange={set('benchPressStartingWeight')} />

        <label htmlFor="ohp">Overhead Press</label>
        <input type="number" id="ohp" min="0" step="2.5" required
          value={form.overheadPressStartingWeight} onChange={set('overheadPressStartingWeight')} />

        <label htmlFor="deadlift">Deadlift</label>
        <input type="number" id="deadlift" min="0" step="2.5" required
          value={form.deadliftStartingWeight} onChange={set('deadliftStartingWeight')} />

        <label htmlFor="altLiftWeight">{altLiftLabel}</label>
        <input type="number" id="altLiftWeight" min="0" step="2.5" required
          value={form.alternatingLiftStartingWeight} onChange={set('alternatingLiftStartingWeight')} />

        <button type="submit" aria-busy={isSubmitting} disabled={isSubmitting}>
          {isSubmitting ? 'Saving…' : 'Start Training'}
        </button>
      </form>
    </article>
  );
}
