using Enums;

namespace Params
{
    public class CharacterActionParam
    {
        public CharacterActionState state;
        public OrderOnEndCharacterAction endOrder;
        public object[] values;

        public CharacterActionParam(CharacterActionState state, OrderOnEndCharacterAction endOrder, params object[] values)
        {
            this.state = state;
            this.endOrder = endOrder;
            this.values = values;
        }
    }
}