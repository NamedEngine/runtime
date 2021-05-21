using System;
using System.Linq;
using UnityEngine;

namespace Language.Variables {
    public class DrawingLayer : SpecialVariable<int> {
        SpriteRenderer _renderer;

        void SetRenderer() {
            _renderer = BoundGameObject.GetComponent<SpriteRenderer>();
        }

        readonly System.Action _setRendererOnce;
        
        protected override int SpecialGet() {
            _setRendererOnce();

            return Convert.ToInt32(_renderer.sortingLayerName);
        }

        protected override void SpecialSet(int value) {
            _setRendererOnce();
            var layerId = SortingLayer.NameToID(value.ToString());
            if (layerId == 0) {
                var layers = SortingLayer.layers
                    .Where(layer => layer.name != "Default")
                    .Select(layer => Convert.ToInt32(layer.name))
                    .ToArray();

                throw new LogicException(nameof(DrawingLayer), $"Invalid sorting layer ({value}):" +
                                                               $"\nvalue should be in range {layers.Min()} - {layers.Max()}");
            }

            _renderer.sortingLayerID = layerId;
        }

        public DrawingLayer(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setRendererOnce = ((System.Action) SetRenderer).Once();
        }
    }
}
