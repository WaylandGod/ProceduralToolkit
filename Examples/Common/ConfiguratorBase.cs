using System.Collections.Generic;
using UnityEngine;

namespace ProceduralToolkit.Examples
{
    public class ConfiguratorBase : MonoBehaviour
    {
        private Palette currentPalette = new Palette();
        private Palette targetPalette = new Palette();

        protected static T InstantiateControl<T>(Transform parent) where T : Component
        {
            T prefab = Resources.Load<T>(typeof(T).Name);
            T control = Instantiate(prefab);
            control.transform.SetParent(parent, false);
            control.transform.localPosition = Vector3.zero;
            control.transform.localRotation = Quaternion.identity;
            control.transform.localScale = Vector3.one;
            return control;
        }

        protected static MeshDraft Platform(float radius, float heignt, int segments = 128)
        {
            float segmentAngle = 360f/segments;
            float currentAngle = 0;

            var lowerRing = new List<Vector3>(segments);
            var upperRing = new List<Vector3>(segments);
            for (var i = 0; i < segments; i++)
            {
                var lowerPoint = PTUtils.PointOnCircle3XZ(radius + heignt, currentAngle);
                lowerRing.Add(lowerPoint + Vector3.down*heignt);

                var upperPoint = PTUtils.PointOnCircle3XZ(radius, currentAngle);
                upperRing.Add(upperPoint);
                currentAngle -= segmentAngle;
            }

            var platform = new MeshDraft {name = "Platform"};
            platform.Add(new MeshDraft()
                .AddTriangleFan(lowerRing)
                .Add(MeshDraft.Band(lowerRing, upperRing))
                .Paint(new Color(0.5f, 0.5f, 0.5f, 1)));

            upperRing.Reverse();
            platform.Add(new MeshDraft()
                .AddTriangleFan(upperRing)
                .Paint(new Color(0.8f, 0.8f, 0.8f, 1)));

            return platform;
        }

        protected static void AssignDraftToMeshFilter(MeshDraft draft, MeshFilter meshFilter, ref Mesh mesh)
        {
            if (mesh == null)
            {
                mesh = draft.ToMesh();
            }
            else
            {
                draft.ToMesh(ref mesh);
            }
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }

        protected static void AssignDraftToMeshFilter(CompoundMeshDraft compoundDraft, MeshFilter meshFilter, ref Mesh mesh)
        {
            if (mesh == null)
            {
                mesh = compoundDraft.ToMeshWithSubMeshes();
            }
            else
            {
                compoundDraft.ToMeshWithSubMeshes(ref mesh);
            }
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }

        protected void GeneratePalette()
        {
            List<ColorHSV> palette = RandomE.TetradicPalette(0.25f, 0.7f);
            targetPalette.mainColor = palette[0].WithSV(0.8f, 0.6f);
            targetPalette.secondaryColor = palette[1].WithSV(0.8f, 0.6f);
            targetPalette.skyColor = palette[2];
            targetPalette.horizonColor = palette[3];
            targetPalette.groundColor = ColorHSV.Lerp(targetPalette.skyColor, targetPalette.horizonColor, 0.5f);
        }

        protected ColorHSV GetMainColorHSV()
        {
            return targetPalette.mainColor;
        }

        protected ColorHSV GetSecondaryColorHSV()
        {
            return targetPalette.secondaryColor;
        }

        protected void SetupSkyboxAndPalette()
        {
            RenderSettings.skybox = new Material(RenderSettings.skybox);
            currentPalette.mainColor = targetPalette.mainColor;
            currentPalette.secondaryColor = targetPalette.secondaryColor;
            currentPalette.skyColor = targetPalette.skyColor;
            currentPalette.horizonColor = targetPalette.horizonColor;
            currentPalette.groundColor = targetPalette.groundColor;
        }

        protected void UpdateSkybox()
        {
            LerpSkybox(RenderSettings.skybox, currentPalette, targetPalette, Time.deltaTime);
        }

        private static void LerpSkybox(Material skybox, Palette currentPalette, Palette targetPalette, float t)
        {
            currentPalette.skyColor = ColorHSV.Lerp(currentPalette.skyColor, targetPalette.skyColor, t);
            currentPalette.horizonColor = ColorHSV.Lerp(currentPalette.horizonColor, targetPalette.horizonColor, t);
            currentPalette.groundColor = ColorHSV.Lerp(currentPalette.groundColor, targetPalette.groundColor, t);

            skybox.SetColor("_SkyColor", currentPalette.skyColor.ToColor());
            skybox.SetColor("_HorizonColor", currentPalette.horizonColor.ToColor());
            skybox.SetColor("_GroundColor", currentPalette.groundColor.ToColor());
        }

        private class Palette
        {
            public ColorHSV mainColor;
            public ColorHSV secondaryColor;
            public ColorHSV skyColor;
            public ColorHSV horizonColor;
            public ColorHSV groundColor;
        }
    }
}
