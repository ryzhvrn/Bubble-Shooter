using UnityEngine;

namespace Watermelon.BubbleShooter
{
    public class BodyHitPoint : HitPoint
    {
        [SerializeField] Type bodyPartType = Type.Unknown;
        public Type BodyPartType => bodyPartType;

        [SerializeField] BodyHitPoint connectedBodyPart;


        private bool isSticked = false;
        public bool IsSticked => isSticked;

        private StickBehaviour stickBehaviour;
        public StickBehaviour StickBehaviour => stickBehaviour;

        private CharacterEnemy characterEnemy;
        public CharacterEnemy CharacterEnemy => characterEnemy;

        public override void Init(BaseEnemy enemy)
        {
            base.Init(enemy);

            characterEnemy = (CharacterEnemy)enemy;
        }

        public override void OnObjectHitted(Vector3 direction, Vector3 hitPosition, BulletData bulletData)
        {
            characterEnemy?.OnBodyPartHitted(this, direction, hitPosition, bulletData);
        }

        public override void SpawnFoam()
        {
            if (foamBehaviour == null)
            {
                foamBehaviour = LevelController.SpawnFoam(foamSpawnTransform.position, 1.3f, 1.5f, foamSpawnTransform);

                stickBehaviour = CreateStickObject(foamBehaviour.gameObject, this);
                stickBehaviour.OnObjectSticked += OnBodyPartSticked;

                if (connectedBodyPart != null)
                    connectedBodyPart.SetFoamBehaviour(foamBehaviour, stickBehaviour);
            }
            else
            {
                foamBehaviour.Resize(1.2f);
            }
        }

        private void OnBodyPartSticked()
        {
            isSticked = true;

            objectRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            stickBehaviour.ObjectRigidbody.constraints = RigidbodyConstraints.FreezeAll;

            stickBehaviour.OnObjectSticked -= OnBodyPartSticked;

            characterEnemy.OnBodyPartSticked(this);

            if (connectedBodyPart != null)
                connectedBodyPart.SetStickState(true);
        }

        public void AddJoint(Rigidbody rigidbody)
        {
            FixedJoint hingeJoint = gameObject.AddComponent<FixedJoint>();
            hingeJoint.connectedBody = rigidbody;
            hingeJoint.autoConfigureConnectedAnchor = false;
            hingeJoint.enableCollision = true;

            objectCollider.enabled = false;
        }

        private StickBehaviour CreateStickObject(GameObject stickGameObject, BodyHitPoint bodyPart)
        {
            StickBehaviour stickBehaviour = stickGameObject.GetComponent<StickBehaviour>();
            if (stickBehaviour == null)
            {
                SphereCollider stickCollider = stickGameObject.AddComponent<SphereCollider>();
                stickCollider.isTrigger = true;
                stickCollider.radius = 1.8f;

                SphereCollider stickCollider2 = stickGameObject.AddComponent<SphereCollider>();
                stickCollider2.isTrigger = false;
                stickCollider2.radius = 1.6f;

                Rigidbody stickRigidbody = stickGameObject.AddComponent<Rigidbody>();
                stickRigidbody.isKinematic = false;

                stickBehaviour = stickGameObject.AddComponent<StickBehaviour>();
                stickBehaviour.Init((CharacterEnemy)enemy);

                // Joint body part to foam
                bodyPart.AddJoint(stickRigidbody);
            }

            return stickBehaviour;
        }

        public void SetPhysicsState(bool state)
        {
            objectRigidbody.isKinematic = !state;
        }

        public void SetFoamBehaviour(FoamBehaviour foamBehaviour, StickBehaviour stickBehaviour)
        {
            this.foamBehaviour = foamBehaviour;
            this.stickBehaviour = stickBehaviour;
        }

        public void SetBodyPartType(Type type)
        {
            bodyPartType = type;
        }

        public void SetConnectedBodyPart(BodyHitPoint bodyPart)
        {
            connectedBodyPart = bodyPart;
        }

        public void SetFoamSpawnPoint(Transform spawnPoint)
        {
            foamSpawnTransform = spawnPoint;
        }

        public void SetStickState(bool state)
        {
            isSticked = state;
        }

        public enum Type
        {
            Unknown = 0,
            Head = 1,
            Hips = 2,
            LeftUpLeg = 3,
            LeftLeg = 4,
            RightUpLeg = 5,
            RightLeg = 6,
            Spine = 7,
            LeftArm = 8,
            LeftForeArm = 9,
            RightArm = 10,
            RightForeArm = 11
        }
    }
}