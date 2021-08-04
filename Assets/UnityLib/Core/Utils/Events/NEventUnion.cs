namespace Nettle {

    public class NEventUnion : NEvent {

        public NEvent[] Events;

        protected override bool Get() {
            foreach (NEvent e in Events) {
                if (e) return true;
            }
            return false;
        }
    }
}
