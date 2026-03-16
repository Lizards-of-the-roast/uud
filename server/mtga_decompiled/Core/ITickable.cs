public interface ITickable
{
	void Update(float deltaTime);

	void PauseTimers();

	void ResumeTimers();
}
