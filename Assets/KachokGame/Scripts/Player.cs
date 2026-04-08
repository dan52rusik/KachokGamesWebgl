using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Tutorial
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float speed = 3f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float reloadTime = 0.6f;
        [SerializeField] private int startHealth = 3;
        [SerializeField] private PlayerUI ui;
        [SerializeField] private Transform hitPoint;
        [SerializeField] private float hitRadius = 1f;
        [SerializeField] private float punchForce = 1f;

        private Rigidbody _rb;
        private CapsuleCollider _capsule;
        private Animator _anim;
        private bool _canHit = true;
        private int _health;
        private bool _hasIsWalk;
        private bool _hasAxeHit;
        private bool _hasSpeed;
        private bool _movementLocked;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();
            _anim = GetComponent<Animator>();
            _health = startHealth;

            EnsureColliderSetup();

            if (_anim != null)
            {
                foreach (AnimatorControllerParameter parameter in _anim.parameters)
                {
                    if (parameter.name == "isWalk")
                        _hasIsWalk = true;
                    else if (parameter.name == "axeHit")
                        _hasAxeHit = true;
                    else if (parameter.name == "Speed")
                        _hasSpeed = true;
                }
            }

            if (ui != null)
                ui.SetHealth(_health);
        }

        private void EnsureColliderSetup()
        {
            if (_capsule == null)
            {
                _capsule = gameObject.AddComponent<CapsuleCollider>();
                _capsule.center = new Vector3(0f, 0.95f, 0f);
                _capsule.height = 1.9f;
                _capsule.radius = 0.32f;
                _capsule.direction = 1;
            }

            if (_rb != null)
            {
                _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        /// <summary>Блокировка/разблокировка управления (для мини-игр)</summary>
        public void SetMovementLocked(bool locked)
        {
            _movementLocked = locked;

            // Остановить персонажа при блокировке
            if (locked)
            {
                if (_rb != null)
                    _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);

                if (_anim != null)
                {
                    if (_hasIsWalk) _anim.SetBool("isWalk", false);
                    if (_hasSpeed) _anim.SetFloat("Speed", 0f);
                }
            }
        }

        private void Update()
        {
            // Если движение заблокировано (тренировка), пропускаем ввод
            if (_movementLocked) return;

            Vector2 input = ReadMoveInput();
            float steer = input.x;
            float moveForward = input.y;
            
            bool isMoving = Mathf.Abs(moveForward) > 0.1f || Mathf.Abs(steer) > 0.1f;

            // Вращаем персонажа: A/D или стрелки влево/вправо
            if (steer != 0f)
            {
                transform.Rotate(0f, steer * rotationSpeed * 15f * Time.deltaTime, 0f);
            }

            // Двигаем персонажа ВПЕРЕД (туда, куда он сейчас смотрит лицом)
            Vector3 velocity = transform.forward * (moveForward * speed);

            if (_anim != null)
            {
                if (_hasIsWalk)
                    _anim.SetBool("isWalk", isMoving);

                if (_hasSpeed)
                    _anim.SetFloat("Speed", isMoving ? 0.5f : 0f); 
            }

            if (_rb != null)
                _rb.linearVelocity = new Vector3(velocity.x, _rb.linearVelocity.y, velocity.z);
            else
                transform.position += velocity * Time.deltaTime;

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed && _canHit)
            {
                if (_anim != null && _hasAxeHit)
                    _anim.SetTrigger("axeHit");

                StartCoroutine(Reload());
            }
        }

        private Vector2 ReadMoveInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return Vector2.zero;

            float x = 0f;
            float y = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)   x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)  x += 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)     y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)   y -= 1f;

            return new Vector2(x, y);
        }

        private IEnumerator Reload()
        {
            _canHit = false;
            yield return new WaitForSeconds(reloadTime);
            _canHit = true;
        }

        public void GetDamage(int damage)
        {
            _health -= damage;

            if (ui != null)
                ui.SetHealth(_health);

            if (_health <= 0)
                SceneManager.LoadScene(0);
        }

        private void Hit()
        {
            Vector3 origin = hitPoint != null
                ? hitPoint.position
                : transform.position + Vector3.up * 1.15f + transform.forward * 0.9f;

            Collider[] colliders = Physics.OverlapSphere(origin, hitRadius);
            for (int i = 0; i < colliders.Length; i++)
            {
                Component bag = FindComponentInParents(colliders[i].transform, "PunchingBag");
                if (bag != null)
                {
                    Vector3 contactPoint = colliders[i].ClosestPoint(origin);
                    var hitMethod = bag.GetType().GetMethod("ApplyHit");
                    if (hitMethod != null)
                        hitMethod.Invoke(bag, new object[] { contactPoint, transform.forward, punchForce });
                    continue;
                }

                Component tree = colliders[i].GetComponent("Tree");
                if (tree != null)
                {
                    bool canDestroy = true;
                    var type = tree.GetType();
                    var respawnedProperty = type.GetProperty("isRespawned");
                    if (respawnedProperty != null)
                        canDestroy = (bool)respawnedProperty.GetValue(tree);

                    if (canDestroy)
                    {
                        if (ui != null)
                            ui.TreeCount++;

                        var destroyMethod = type.GetMethod("Destroy", Type.EmptyTypes);
                        if (destroyMethod != null)
                            destroyMethod.Invoke(tree, null);
                        else
                            Destroy(colliders[i].gameObject);
                    }

                    continue;
                }

                if (colliders[i].GetComponent("Enemy") != null)
                    Destroy(colliders[i].gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 origin = hitPoint != null
                ? hitPoint.position
                : transform.position + Vector3.up * 1.15f + transform.forward * 0.9f;
            Gizmos.DrawWireSphere(origin, hitRadius);
        }

        private static Component FindComponentInParents(Transform start, string componentName)
        {
            Transform current = start;
            while (current != null)
            {
                Component component = current.GetComponent(componentName);
                if (component != null)
                    return component;
                current = current.parent;
            }

            return null;
        }
    }
}
