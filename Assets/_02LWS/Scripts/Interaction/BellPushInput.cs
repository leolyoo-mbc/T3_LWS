using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using LWS.FireNotification.Domain;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Interaction
{
    // The collider/filter stay fixed; only the visible cap moves. Depth is measured in surface-local +Z.
    public sealed class BellPushInput : MonoBehaviour, IXRHoverFilter, IXRSelectFilter
    {
        public FireNotificationController controller;
        public XRSimpleInteractable interactable;
        public XRPokeFilter pokeFilter;
        public Transform surface;
        public Transform cap;
        [Min(0.005f)] public float pressDepth=0.025f;
        [Tooltip("Radius of the circular press plate, in surface-local metres.")]
        [Min(0.02f)] public float halfWidth=0.12f;
        public float contactTolerance=0.004f;
        public float maximumDepth=0.09f;
        public float releaseDistance=0.015f;
        public bool Armed { get; private set; }
        public bool Pressed { get; private set; }
        public float NormalizedDepth { get; private set; }
        private struct Contact { public float previousDepth; public bool touched; public bool initialized; }
        private readonly Dictionary<XRPokeInteractor,Contact> contacts=new Dictionary<XRPokeInteractor,Contact>();
        private XRPokeInteractor[] pokes;
        private Vector3 restCap;
        private bool initialized;
        private ExperienceSession observedSession;
        private long observedExecution = -1;
        private bool pausedByMenu;
        private bool requireRelease;
        public bool canProcess=>isActiveAndEnabled;
        public bool Process(IXRHoverInteractor interactor,IXRHoverInteractable target)=>interactor is XRPokeInteractor;
        public bool Process(IXRSelectInteractor interactor,IXRSelectInteractable target)=>interactor is XRPokeInteractor;
        private void Awake()
        {
            restCap=cap.localPosition;initialized=true;
            interactable.hoverFilters.Add(this);interactable.selectFilters.Add(this);
        }
        private void Start()
        {
            // Starter Assets enables controllers only after a tracked device is available.
            pokes=FindObjectsByType<XRPokeInteractor>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            controller.StateChanged+=OnStateChanged;OnStateChanged();
        }
        private void OnStateChanged()
        {
            var session = controller.Session;
            if (session != observedSession || session == null || session.ExecutionId != observedExecution)
            {
                observedSession = session;
                observedExecution = session != null ? session.ExecutionId : -1;
                pausedByMenu = controller.UserPaused;
                requireRelease = false;
                ResetPush();
                return;
            }
            if (controller.UserPaused) { pausedByMenu = true; return; }
            if (pausedByMenu)
            {
                // Keep the frozen pose, but discard old contact eligibility on continue.
                pausedByMenu = false;
                requireRelease = true;
                Armed = false; Pressed = false; contacts.Clear();
            }
        }
        private void ResetPush()
        {
            Armed=false;Pressed=false;NormalizedDepth=0;contacts.Clear();
            if(cap!=null&&initialized)cap.localPosition=restCap;
        }
        private void Update()
        {
            if(pokes==null || controller.Session==null)return;
            var s=controller.Session;
            if(controller.InputBlocked||s.Stage!=ExperienceStage.AwaitBellPress) return;
            bool anyContact=false;float depth=0;
            foreach(var poke in pokes)
            {
                if(poke==null||!poke.isActiveAndEnabled)continue;
                var local=surface.InverseTransformPoint(poke.GetAttachTransform(interactable).position);
                bool lateral=local.x*local.x+local.y*local.y<=halfWidth*halfWidth;
                bool touching=lateral&&local.z>=-releaseDistance&&local.z<=maximumDepth;
                anyContact|=touching;
                if(requireRelease)continue;
                contacts.TryGetValue(poke,out var contact);
                if(!touching) contact.touched=false;
                bool hover=interactable.interactorsHovering.Contains(poke);
                if(Armed&&contact.initialized&&lateral&&hover&&contact.previousDepth < 0 && local.z>=-contactTolerance && local.z>contact.previousDepth)
                    contact.touched=true;
                if(Armed&&contact.touched&&touching&&hover)
                {
                    depth=Mathf.Max(depth,Mathf.Max(0,local.z));
                    if(!Pressed&&local.z>=pressDepth)
                    {
                        Pressed=true;Armed=false;controller.Send(ExperienceCommand.PressBell);
                    }
                }
                contact.previousDepth=local.z;contact.initialized=true;contacts[poke]=contact;
            }
            if(requireRelease)
            {
                if(!anyContact) { requireRelease=false; ResetPush(); Armed=true; }
                return;
            }
            if(!anyContact&&!Pressed)Armed=true;
            NormalizedDepth=Mathf.Clamp01(depth/pressDepth);
            cap.localPosition=restCap+Vector3.forward*Mathf.Clamp(depth,0,pressDepth);
        }
        private void OnDisable()
        {
            if(initialized)ResetPush();
            observedSession=null;observedExecution=-1;pausedByMenu=false;requireRelease=false;
        }
        private void OnDestroy()
        {
            if(controller!=null)controller.StateChanged-=OnStateChanged;
            if(interactable!=null){interactable.hoverFilters.Remove(this);interactable.selectFilters.Remove(this);}
        }
    }
}
