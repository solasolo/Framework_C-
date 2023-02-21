namespace SoulFab.Core.Data
{
    public enum TriggerType
    {
        Rising,
        None,
        Falling,
    }

    public class Trigger
    {
        private bool PreValue;

        public Trigger(bool value = false)
        {
            PreValue = value;
        }

        public TriggerType Check(bool value)
        { 
            TriggerType ret = TriggerType.None;

            if(value != this.PreValue)
            {
                ret = value ? TriggerType.Rising : TriggerType.Falling;
            }
            
            this.PreValue = value;

            return ret;
        }
    }
}