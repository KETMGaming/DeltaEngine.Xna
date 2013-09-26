﻿using System;
using CreepyTowers.Towers;
using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Rendering2D.Sprites;
using DeltaEngine.Rendering3D.Models;

namespace CreepyTowers.Creeps
{
	/// <summary>
	/// Enemy creature, its features determined by the type given to the constructor.
	/// </summary>
	public class Creep : Model, IDisposable
	{
		public Creep(Vector3D position, string name, CreepProperties creepProperties)
			: base(name, position)
		{
			Position = position;
			SetupHealthBar();
			SetDefaultValues();
			Add(creepProperties);
			Start<MovementInGrid>();
			state = new CreepState();
			creepStateChanger = new CreepStateChanger();
			creepStateChanger.SetStartStateOfCreep(creepProperties.CreepType, this);
			calculateDamage = new CalculateDamage();
		}

		public CreepState state;
		private readonly CreepStateChanger creepStateChanger;
		private readonly CalculateDamage calculateDamage;

		private void SetupHealthBar()
		{
			HealthBar = new Sprite(new Material(Shader.Position2DUv, Names.ImageHealthBarGreen100),
				Rectangle.Zero);
			HealthBar.RenderLayer = (int)CreepyTowersRenderLayer.Interface;
		}

		private void SetDefaultValues()
		{
			RenderLayer = (int)CreepyTowersRenderLayer.Creeps;
		}

		public Sprite HealthBar { get; set; }
		public readonly Size HealthBarSize = new Size(0.04f, 0.005f);

		public enum CreepType
		{
			Paper,
			Cloth,
			Wood,
			Plastic,
			Sand,
			Glass,
			Iron
		}

		public void ReceiveAttack(Tower.TowerType damageType, float rawDamage)
		{
			var properties = Get<CreepProperties>();
			creepStateChanger.CheckIfChangingCreepState(damageType, this, properties);
			if (!IsActive)
				return;
			var interactionEffect = calculateDamage.CalculateResistanceBasedOnStates(damageType, this);
			float dmg;
			if (state.Enfeeble)
				dmg = (rawDamage - (properties.Resistance / 2)) * interactionEffect;
			else
				dmg = (rawDamage - properties.Resistance) * interactionEffect;
			properties.CurrentHp -= dmg;
			if (properties.CurrentHp <= 0.0f)
				Die();
		}

		private void Die()
		{
			//DisplayCreepDieEffect();

			if (CreepIsDead != null)
				CreepIsDead();

			Dispose();
		}

		public event Action CreepIsDead;

		private void DisplayCreepDieEffect()
		{
			//var creep2DPoint = Game.CameraAndGrid.GameCamera.WorldToScreenPoint(Position);
			//var drawArea = Rectangle.FromCenter(creep2DPoint, new Size(0.07f));
			//DyingEffect = new SpriteSheetAnimation("SpriteDyingCloud", drawArea);
			//DyingEffect.RenderLayer = (int)CreepyTowersRenderLayer.Interface;
			//DyingEffect.FinalFrame += RemoveDyingEffect;
		}

		public SpriteSheetAnimation DyingEffect { get; private set; }

		public void UpdateHealthBarPositionAndImage()
		{
			if (UpdateHealthBar != null)
				UpdateHealthBar();
		}

		public event Action UpdateHealthBar;

		public void Dispose()
		{
			if (HealthBar != null)
				HealthBar.IsActive = false;

			IsActive = false;
		}

		private void RemoveDyingEffect()
		{
			//if (DyingEffect != null)
			//	DyingEffect.IsActive = false;
		}

		// ToDo: Somehow feels like the time logic doesnt work or that the logic is wrong. Needs to be checked.
		public void UpdateStateTimersAndTimeBasedDamage()
		{
			var properties = Get<CreepProperties>();
			if (state.Burst)
				if (Time.CheckEvery(1))
					properties.CurrentHp -= properties.MaxHp / 12;
			if (state.Burn)
				if (Time.CheckEvery(1))
					properties.CurrentHp -= properties.MaxHp / 16;
			UpdateTimers();
		}

		private void UpdateTimers()
		{
			if (state.Slow)
				state.Slow = state.UpdateSlowState(state.SlowTimer, state.MaxTimeMedium);
			if (state.Delayed)
				state.Delayed = state.UpdateSlowState(state.DelayedTimer, state.MaxTimeMedium);
			if (state.Burn)
				state.Burn = state.UpdateSlowState(state.BurnTimer, state.MaxTimeMedium);
			if (state.Burst)
				state.Burst = state.UpdateSlowState(state.BurstTimer, state.MaxTimeMedium);
			if (state.Paralysed)
				state.Paralysed = state.UpdateSlowState(state.ParalysedTimer, state.MaxTimeShort);
			if (state.Frozen)
				state.Frozen = state.UpdateSlowState(state.FrozenTimer, state.MaxTimeShort);
			if (state.Fast)
				state.Fast = state.UpdateSlowState(state.FastTimer, state.MaxTimeMedium);
			if (state.Enfeeble)
				state.Enfeeble = state.UpdateSlowState(state.EnfeebleTimer, state.MaxTimeMedium);
			if (state.Melt)
				state.Melt = state.UpdateSlowState(state.MeltTimer, state.MaxTimeLong);
			if (state.Rust)
				state.Rust = state.UpdateSlowState(state.RustTimer, state.MaxTimeLong);
			if (state.Wet)
				state.Wet = state.UpdateSlowState(state.WetTimer, state.MaxTimeLong);
		}

		public void Shatter()
		{
			var creepList = EntitiesRunner.Current.GetEntitiesOfType<Creep>();
			foreach (Creep creep in creepList)
			{
				if (creep == this)
					continue;

				var distance = ((creep.Position.X - Position.X) * (creep.Position.X - Position.X)) +
					((creep.Position.Y - Position.Y) * (creep.Position.Y - Position.Y));

				if (distance > 4)
					return;

				var properties = creep.Get<CreepProperties>();
				properties.CurrentHp -= 40;
			}
		}
	}
}