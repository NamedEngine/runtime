using System.Collections.Generic;
using Language.Variables;
using Variables;

namespace Language.Classes {
    public class Empty : LogicObject {
        public const string EmptyClassName = "";

        public Empty() {
            ThisClassVariables = new Dictionary<string, IVariable> {
                {nameof(Visible), new Visible(null, null)},
                {nameof(Collidable), new Collidable(null, null)},
                {nameof(X), new X(null, null)},
                {nameof(Y), new Y(null, null)},
                {nameof(Rotation), new Rotation(null, null)},
                {nameof(ScaleX), new ScaleX(null, null)},
                {nameof(ScaleY), new ScaleY(null, null)},
                {nameof(SizeX), new SizeX(null, null)},
                {nameof(SizeY), new SizeY(null, null)},
                {nameof(VelocityX), new VelocityX(null, null)},
                {nameof(VelocityY), new VelocityY(null, null)},
            };
            
            ObjectClass = EmptyClassName;
        }
    }
}
