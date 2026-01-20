namespace CoffeeShop.ViewModels.AdminVM
{
    public class EventAggregator
    {
        private static EventAggregator? _instance;
        public static EventAggregator Instance => _instance ??= new EventAggregator();

        private readonly Dictionary<Type, List<Action<object>>> _subscriptions = new();

        public void Subscribe<TMessage>(Action<TMessage> action)
        {
            var messageType = typeof(TMessage);
            if (!_subscriptions.ContainsKey(messageType))
                _subscriptions[messageType] = new List<Action<object>>();

            _subscriptions[messageType].Add(obj => action((TMessage)obj));
        }

        public void Unsubscribe<TMessage>(Action<TMessage> action)
        {
            var messageType = typeof(TMessage);
            if (_subscriptions.ContainsKey(messageType))
            {
                // Tìm các subscription có mục tiêu (target) trùng với action truyền vào
                var handlers = _subscriptions[messageType];
                var itemToRemove = handlers.FirstOrDefault(h => h.Target == action.Target && h.Method == action.Method);

                if (itemToRemove != null)
                {
                    handlers.Remove(itemToRemove);
                }
            }
        }

        public void Publish<TMessage>(TMessage message)
        {
            var messageType = typeof(TMessage);
            if (_subscriptions.ContainsKey(messageType))
            {
                foreach (var action in _subscriptions[messageType])
                    action(message!);
            }
        }
    }
}
