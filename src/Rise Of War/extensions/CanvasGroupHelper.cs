﻿
using UnityEngine;

namespace RiseOfWar
{
    public static class CanvasGroupHelper
    {
        public static void SetActive(this CanvasGroup canvasGroup, bool active)
        {
            canvasGroup.alpha = (active) ? 1 : 0;
            canvasGroup.interactable = active;
            canvasGroup.blocksRaycasts = active;
        }
    }
}