using UnityEngine;

namespace TripleMatch.Board
{
    /// <summary>
    /// Camera controller for the gameplay board. Drives pan + pinch-zoom and clamps the
    /// camera frustum view so the view never extends outside the supplied bounds.
    /// </summary>
    public interface ICameraController
    {
        /// <summary>
        /// Set the world-space rectangle the camera must stay inside
        /// Called by BoardManager after background sprite assigned.
        /// </summary>
        void SetBounds(Vector3 worldCenter, Vector2 worldSize);
    }
}
