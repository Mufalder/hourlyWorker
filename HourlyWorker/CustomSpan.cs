using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HourlyWorker
{

    /// <summary>
    /// Custom TimeSpan class.
    /// </summary>
    [Serializable]
    public class CustomSpan
    {

        /// <summary>
        /// Backing field for the <see cref="Hours"/>.
        /// </summary>
        [JsonIgnore]
        private int hours = 0;
        /// <summary>
        /// How many hours have been passed.
        /// </summary>
        public int Hours
        {
            get => hours;
            private set
            {
                if (hours == value)
                    return;

                hours = value;

                if (hours < 0)
                    hours = 0;

                onChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Backing field for the <see cref="Minutes"/>.
        /// </summary>
        [JsonIgnore]
        private int minutes = 0;
        /// <summary>
        /// How many minutes have been passed,
        /// </summary>
        public int Minutes
        {
            get => minutes;
            private set
            {
                if (minutes == value)
                    return;

                minutes = value;

                if (minutes < 0)
                    minutes = 0;
                else if (minutes >= 60)
                {
                    minutes = 0;
                    Hours++;
                }

                onChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Change event.
        /// </summary>
        public delegate void OnChanged(CustomSpan sender);
        public OnChanged onChanged;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CustomSpan()
        {
        }

        /// <summary>
        /// Specific constructor.
        /// </summary>
        [JsonConstructor]
        public CustomSpan(int hours, int minutes)
        {
            Hours = hours;
            Minutes = minutes;
        }

        /// <summary>
        /// Creates a CustomSpan from the string.
        /// </summary>
        public CustomSpan(string input)
        {
            string[] seperated = input.Split(':');
            if (int.TryParse(seperated[0], out int hours))
            {
                this.hours = hours;
            }
            if (int.TryParse(seperated[1], out int minutes))
            {
                this.minutes = minutes;
            }
        }

        /// <summary>
        /// Converts CustomSpan to the formatted string.
        /// Format: HH:MM
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}:{1}", Hours.ToString("00"), Minutes.ToString("00"));
        }

        /// <summary>
        /// Add hours to the span. Can be negative.
        /// </summary>
        public void AddHours(int add = 1)
        {
            Hours += add;
        }

        /// <summary>
        /// Add minutes to the span. Can be negative.
        /// </summary>
        public void AddMinutes(int add = 1)
        {
            Minutes += add;
        }
    }
}
