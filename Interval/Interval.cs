using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Globalization;

namespace ExtendedArithmetic
{
	/// <summary> 
	/// Interval arithmetic class
	/// </summary>
	public class Interval<T>
		: IAdditionOperators<Interval<T>, Interval<T>, Interval<T>>,
		  ISubtractionOperators<Interval<T>, Interval<T>, Interval<T>>,
		  IMultiplyOperators<Interval<T>, Interval<T>, Interval<T>>,
		  IDivisionOperators<Interval<T>, Interval<T>, Interval<T>>,
		  IEqualityOperators<Interval<T>, Interval<T>, bool>,
		  IComparisonOperators<Interval<T>, Interval<T>, bool>,
		  IUnaryNegationOperators<Interval<T>, Interval<T>>,
		  IEquatable<Interval<T>>, IComparable<Interval<T>>, IComparable, IFormattable
		where T : IAdditionOperators<T, T, T>,
				  ISubtractionOperators<T, T, T>,
				  IMultiplyOperators<T, T, T>,
				  IDivisionOperators<T, T, T>,
				  IEqualityOperators<T, T, bool>,
				  IComparisonOperators<T, T, bool>,
				  IParsable<T>,
				  IFormattable
	{
		public static Interval<T> Zero;
		public static Interval<T> One;

		public T Min { get; private set; }
		public T Max { get; private set; }

		public T Size { get { return Max - Min; } }
		public T ArithmeticAverage { get { return (Min + Max) / T.Parse("2", NumberFormatInfo.CurrentInfo); } }

		#region Constructors

		public Interval() : this(T.Parse("0", NumberFormatInfo.CurrentInfo)) { }
		public Interval(T value) : this(value, value) { }
		public Interval(T min, T max)
		{
			Min = min;
			Max = max;
		}

		static Interval()
		{
			Zero = new Interval<T>(T.Parse("0", NumberFormatInfo.CurrentInfo));
			One = new Interval<T>(T.Parse("1", NumberFormatInfo.CurrentInfo));
		}

		public static Interval<T> Combine(Interval<T> left, Interval<T> right)
		{
			if (Interval<T>.IsDisjoint(left, right)) { throw new ArgumentOutOfRangeException($"Parameters {nameof(left)} and {nameof(right)} must overlap."); }

			Interval<T> first = left;
			Interval<T> second = right;
			if (right < left)
			{
				first = right;
				second = left;
			}
			return new Interval<T>(CollectionMin(first.Min, second.Min), CollectionMax(first.Max, second.Max));
		}

		#endregion

		#region Operations

		public static Interval<T> Add(Interval<T> left, Interval<T> right) => new Interval<T>((left.Min + right.Min), (left.Max + right.Max));
		public static Interval<T> Subtract(Interval<T> left, Interval<T> right) => new Interval<T>((left.Min - right.Max), (left.Max - right.Min));
		public static Interval<T> Divide(Interval<T> left, Interval<T> right) => new Interval<T>((left.Min / right.Max), (left.Max / right.Min));
		public static Interval<T> Multiply(Interval<T> left, Interval<T> right) =>
			new Interval<T>(
				CollectionMin(left.Min * right.Min, left.Min * right.Max, left.Max * right.Min, left.Max * right.Max),
				CollectionMax(left.Min * right.Min, left.Min * right.Max, left.Max * right.Min, left.Max * right.Max)
			);
		public static Interval<T> Negate(Interval<T> value) => Subtract(Interval<T>.Zero, value);

		public static Interval<T> Clone(Interval<T> value) => new Interval<T>(value.Min, value.Max);

		private static T CollectionMin(params T[] elements) { return elements.Min(); }
		private static T CollectionMax(params T[] elements) { return elements.Max(); }

		#endregion

		#region Operators

		#region Arithmetic Operators

		public static Interval<T> operator +(Interval<T> left, Interval<T> right) => Interval<T>.Add(left, right);

		public static Interval<T> operator -(Interval<T> left, Interval<T> right) => Interval<T>.Subtract(left, right);

		public static Interval<T> operator *(Interval<T> left, Interval<T> right) => Interval<T>.Multiply(left, right);

		public static Interval<T> operator /(Interval<T> left, Interval<T> right) => Interval<T>.Divide(left, right);

		public static Interval<T> operator -(Interval<T> value) => Interval<T>.Negate(value);

		#endregion

		#region Comparison Operators

		public static bool operator ==(Interval<T> left, Interval<T> right) => Interval<T>.Equals(left, right);

		public static bool operator !=(Interval<T> left, Interval<T> right) => !Interval<T>.Equals(left, right);

		public static bool operator <(Interval<T> left, Interval<T> right) => (left.Min <= right.Min) && (left.Max < right.Max);

		public static bool operator >(Interval<T> left, Interval<T> right) => (right.Max >= left.Max) && (right.Min > left.Min);

		public static bool operator <=(Interval<T> left, Interval<T> right) => (left < right) || (left.Max == right.Max);

		public static bool operator >=(Interval<T> left, Interval<T> right) => (right > left) || (right.Min == left.Min);

		#endregion

		#region Conversion Operators

		public static implicit operator Interval<T>(T value) => new Interval<T>(value);

		#endregion

		#endregion

		#region Equality / CompareTo

		public static bool Equals(Interval<T> left, Interval<T> right)
		{
			return (left.Min == right.Min && left.Max == right.Max);
		}

		public bool Equals(Interval<T> other)
		{
			return this.Equals(other);
		}

		public int CompareTo(object obj)
		{
			return this.CompareTo(obj as Interval<T>);
		}

		public int CompareTo(Interval<T> other)
		{
			if (Interval<T>.Equals(this, other)) { return 0; }
			else if (this > other) { return 1; }
			else if (this < other) { return -1; }
			else if (this >= other) { return 1; }
			else if (this <= other) { return -1; }
			else { throw new Exception(); }
		}

		#endregion

		#region Membership

		public bool Contains(T value)
		{
			return (value >= this.Min && value <= this.Max);
		}

		public static bool IsDisjoint(Interval<T> left, Interval<T> right)
		{
			return !(left.Contains(right.Min) || left.Contains(right.Max));
		}

		#endregion

		#region Overrides

		public override bool Equals(object obj)
		{
			return Interval<T>.Equals(this, obj as Interval<T>);
		}

		public override int GetHashCode()
		{
			return new Tuple<T, T>(this.Min, this.Max).GetHashCode();
		}

		public override string ToString()
		{
			return ToString(null, NumberFormatInfo.CurrentInfo);
		}

		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			if (Min == Max)
			{
				return Min.ToString(format, formatProvider);
			}
			else
			{
				return $"[{Min.ToString(format, formatProvider)},{Max.ToString(format, formatProvider)}]";
			}
		}

		#endregion

	}
}
