using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HourlyWorker
{

    /// <summary>
    /// Data for the work projects.
    /// </summary>
    [Serializable]
    public class WorkProject : INotifyPropertyChanged
    {

        /// <summary>
        /// Name of the project.
        /// </summary>
        public string Name { get; set; } = default;

        /// <summary>
        /// Work timespan.
        /// </summary>
        public CustomSpan TimeSpan { get; private set; } = new CustomSpan();
        
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <inheritdoc/>
        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Backing field for the Counting. <see cref="Counting"/>.
        /// </summary>
        [JsonIgnore]
        private bool counting = false;
        /// <summary>
        /// Is timer started for this project?
        /// </summary>
        [JsonIgnore]
        public bool Counting
        {
            get => counting;
            set
            {
                if (counting == value)
                    return;

                counting = value;
                //Raise changed event
                RaisePropertyChanged(nameof(Counting));
            }
        }

        /// <summary>
        /// Backing field for the MinutesCount. <see cref="MinutesCount"/>.
        /// </summary>
        [JsonIgnore]
        private int minutesCount = 0;
        /// <summary>
        /// Minutes count for the timer. This property will invoke notification if an hour is passed.
        /// </summary>
        [JsonIgnore]
        public int MinutesCount
        {
            get => minutesCount;
            set
            {
                minutesCount = value;

                //Invoke notification that an hour is passed and reset count.
                if (minutesCount >= 60)
                {
                    MainWindow mainWindow = (MainWindow)App.Current.MainWindow;
                    mainWindow.ShowNotification(new Notifications.Wpf.NotificationContent
                    {
                        Title = Name,
                        Message = string.Format("An hour passed on the {0} project.", Name),
                        Type = Notifications.Wpf.NotificationType.Information
                    });
                    minutesCount = 0;
                }
            }
        }

        /// <summary>
        /// Constructor for this class.
        /// </summary>
        [JsonConstructor]
        public WorkProject(string name, CustomSpan timeSpan)
        {
            Name = name;
            TimeSpan = timeSpan ?? new CustomSpan();
            TimeSpan.onChanged += TimeSpanChanged;
        }

        /// <summary>
        /// Reaction on time span change.
        /// </summary>
        public void TimeSpanChanged(CustomSpan sender)
        {
            //Raise changed event
            RaisePropertyChanged(nameof(TimeSpan));
        }

        /// <summary>
        /// Start timer count.
        /// </summary>
        public void Start()
        {
            Counting = true;
        }

        /// <summary>
        /// Update timespan.
        /// </summary>
        public void Update()
        {
            if (!Counting)
                return;

            TimeSpan.AddMinutes();
            MinutesCount++;
        }

        /// <summary>
        /// Stop timer.
        /// </summary>
        public void Stop()
        {
            MinutesCount = 0;
            Counting = false;
        }

    }
}
