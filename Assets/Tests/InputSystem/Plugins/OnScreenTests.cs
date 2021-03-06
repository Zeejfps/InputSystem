using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.TestTools.Utils;

internal class OnScreenTests : InputTestFixture
{
    [Test]
    [Category("Devices")]
    public void Devices_CanCreateOnScreenStick()
    {
        var gameObject = new GameObject();
        var stickObject = new GameObject();
        gameObject.AddComponent<Camera>();
        var canvas = gameObject.AddComponent<Canvas>();
        var eventSystem = gameObject.AddComponent<EventSystem>();

        stickObject.AddComponent<RectTransform>();
        var stick = stickObject.AddComponent<OnScreenStick>();
        stick.transform.SetParent(canvas.transform);
        stick.controlPath = "/<Gamepad>/leftStick";

        Assert.That(stick.control.device, Is.TypeOf<Gamepad>());
        Assert.That(stick.control, Is.SameAs(stick.control.device["leftStick"]));
        Assert.That(stick.control, Is.TypeOf<StickControl>());
        var stickControl = (StickControl)stick.control;

        stick.OnDrag(new PointerEventData(eventSystem)
        {
            position = new Vector2(stick.movementRange, stick.movementRange)
        });

        InputSystem.Update();

        Assert.That(stick.control.ReadValueAsObject(),
            Is.EqualTo(stickControl.ProcessValue(new Vector2(stick.movementRange / 2f, stick.movementRange / 2f)))
                .Using(Vector2EqualityComparer.Instance));

        Assert.That(stickObject.transform.position.x, Is.GreaterThan(0.0f));
        Assert.That(stickObject.transform.position.y, Is.GreaterThan(0.0f));
    }

    [Test]
    [Category("Devices")]
    public void Devices_CanCreateOnScreenButton()
    {
        var gameObject = new GameObject();
        var button = gameObject.AddComponent<OnScreenButton>();
        button.controlPath = "/<Keyboard>/a";

        Assert.That(InputSystem.devices, Has.Exactly(1).InstanceOf<Keyboard>());
        var keyboard = (Keyboard)InputSystem.devices.FirstOrDefault(x => x is Keyboard);

        Assert.That(keyboard.aKey.isPressed, Is.False);

        button.OnPointerDown(null);
        InputSystem.Update();
        Assert.That(keyboard.aKey.isPressed, Is.True);

        button.OnPointerUp(null);
        InputSystem.Update();
        Assert.That(keyboard.aKey.isPressed, Is.False);
    }

    // When we receive the OnPointerDown event in OnScreenButton, someone may disable the button as a response.
    // In that case, we don't get an OnPointerUp. Ensure that the OnScreenButton correctly resets the state of
    // its InputControl when the button is enabled.
    [Test]
    [Category("Devices")]
    public void Devices_CanDisableOnScreenButtonFromPressEvent()
    {
        var gameObject = new GameObject();
        var button = gameObject.AddComponent<OnScreenButton>();
        button.controlPath = "<Keyboard>/a";

        // Add a second button so that the device doesn't go away when we disable
        // the first one.
        new GameObject().AddComponent<OnScreenButton>().controlPath = "<Keyboard>/b";

        // When we disable the OnScreenComponent, the keyboard goes away, so use a state monitor
        // to observe the change.
        bool? isPressed = null;
        InputState.AddChangeMonitor(Keyboard.current.aKey,
            (control, time, eventPtr, index) =>
            {
                isPressed = ((ButtonControl)control).isPressed;
            });

        button.OnPointerDown(null);
        InputSystem.Update();

        Assert.That(isPressed, Is.True);

        isPressed = null;
        gameObject.SetActive(false);
        InputSystem.Update();

        Assert.That(isPressed, Is.False);
    }

    ////TODO: we should allow this as an optional feature
    [Test]
    [Category("Devices")]
    public void Devices_OnScreenControlsDoNotUseExistingDevices()
    {
        var existingKeyboard = InputSystem.AddDevice<Keyboard>();
        var gameObject = new GameObject();
        var button = gameObject.AddComponent<OnScreenButton>();
        button.controlPath = "/<Keyboard>/a";

        Assert.That(existingKeyboard.aKey.isPressed, Is.False);
        button.OnPointerDown(null);
        InputSystem.Update();
        Assert.That(existingKeyboard.aKey.isPressed, Is.False);
    }

    [Test]
    [Category("Devices")]
    public void Devices_OnScreenControlsShareDevicesOfTheSameType()
    {
        var gameObject = new GameObject();
        var aKey = gameObject.AddComponent<OnScreenButton>();
        var bKey = gameObject.AddComponent<OnScreenButton>();
        var leftTrigger = gameObject.AddComponent<OnScreenButton>();

        aKey.controlPath = "/<Keyboard>/a";
        bKey.controlPath = "/<Keyboard>/b";
        leftTrigger.controlPath = "/<Gamepad>/leftTrigger";

        Assert.That(aKey.control.device, Is.SameAs(bKey.control.device));
        Assert.That(aKey.control.device, Is.Not.SameAs(leftTrigger.control.device));
        Assert.That(bKey.control.device, Is.Not.SameAs(leftTrigger.control.device));
    }

    [Test]
    [Category("Devices")]
    public void Devices_DisablingLastOnScreenControlRemovesCreatedDevice()
    {
        var gameObject = new GameObject();
        var buttonA = gameObject.AddComponent<OnScreenButton>();
        var buttonB = gameObject.AddComponent<OnScreenButton>();
        buttonA.controlPath = "/<Keyboard>/a";
        buttonB.controlPath = "/<Keyboard>/b";

        Assert.That(InputSystem.devices, Has.Exactly(1).InstanceOf<Keyboard>());

        buttonA.enabled = false;

        Assert.That(InputSystem.devices, Has.Exactly(1).InstanceOf<Keyboard>());

        buttonB.enabled = false;

        Assert.That(InputSystem.devices, Has.None.InstanceOf<Keyboard>());
    }
}
