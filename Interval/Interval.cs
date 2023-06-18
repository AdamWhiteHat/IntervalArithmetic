using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Globalization;

namespace ExtendedArithmetic
{

	/// <summary> 
	/// An Interval numeric data type. 
	/// Instead of representing a value as a single number, an interval represents each value as a range of possibilities 
	/// that you can performs arithmetic on.
	/// 

	public class Interval<T>
		: IAdditionOperators<Interval<T>, Interval<T>, Interval<T>>,
		  ISubtractionOperators<Interval<T>, Interval<T>, Interval<T>>,
		  IMultiplyOperators<Interval<T>, Interval<T>, Interval<T>>,
		  IDivisionOperators<Interval<T>, Interval<T>, Interval<T>>,
		  IEqualityOperators<Interval<T>, Interval<T>, bool>,
		  IComparisonOperators<Interval<T>, Interval<T>, bool>,
		  IEquatable<Interval<T>>,
		  IComparable<Interval<T>>,
		  IComparable
		//IFormattable
		where T :
				  INumberBase<T>,
				  IAdditionOperators<T, T, T>,
				  ISubtractionOperators<T, T, T>,
				  IMultiplyOperators<T, T, T>,
				  IDivisionOperators<T, T, T>,
				  IEqualityOperators<T, T, bool>,
				  IComparisonOperators<T, T, bool>,
				  IParsable<T>,
				  IFormattable
	{
		/// <summary>
		/// Gets a value that represents the number zero as a single-point interval, [0,0].
		/// </summary>
		public static Interval<T> Zero;

		/// <summary>
		/// Gets a value that represents the number one as a single-point interval, [1,1].
		/// </summary>
		public static Interval<T> One;

		/// <summary>
		/// Represents the lower bound of this interval.
		/// </summary>
		/// <value>The minimum.</value>
		public T Min { get; private set; }

		/// <summary>
		/// Represents the upper bound of this interval.
		/// </summary>
		/// <value>The maximum.</value>
		public T Max { get; private set; }

		/// <summary>
		/// Gets the size of this interval.
		/// Calculated as the minimum subtracted from the maximum.
		/// </summary>
		/// <value>The size.</value>
		public T Size { get { return Max - Min; } }

		/// <summary>
		/// Gets the arithmetic average of this interval.
		/// </summary>
		/// <value>The arithmetic average.</value>
		public T ArithmeticAverage { get { return (Min + Max) / T.Parse("2", NumberFormatInfo.CurrentInfo); } }

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Interval{T}"/> class.
		/// </summary>
		public Interval()
		{
			Min = T.Parse(NumberFormatInfo.InvariantInfo.NegativeInfinitySymbol, NumberFormatInfo.InvariantInfo);
			Max = T.Parse(NumberFormatInfo.InvariantInfo.PositiveInfinitySymbol, NumberFormatInfo.InvariantInfo);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Interval{T}"/> class as a single-point interval.
		/// </summary>
		/// <param name="value">The value.</param>
		public Interval(T value) : this(value, value) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Interval{T}"/> class, with the minimum and the maximum specified.
		/// </summary>
		/// <param name="min">The minimum.</param>
		/// <param name="max">The maximum.</param>
		public Interval(T min, T max)
		{
			Min = min;
			Max = max;
		}

		/// <summary>
		/// Static constructor. Initializes the static members of the <see cref="Interval{T}"/> class.
		/// </summary>
		static Interval()
		{
			Zero = new Interval<T>(T.Parse("0", NumberFormatInfo.CurrentInfo));
			One = new Interval<T>(T.Parse("1", NumberFormatInfo.CurrentInfo));
		}

		/// <summary>
		/// Combines two intervals that meet. Same as a union, but the intervals must meet.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>Interval&lt;T&gt;.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Parameters {nameof(left)} and {nameof(right)} must meet.</exception>
		public static Interval<T> Combine(Interval<T> left, Interval<T> right)
		{
			if (Interval<T>.IsDisjoint(left, right)) { throw new ArgumentOutOfRangeException($"Parameters {nameof(left)} and {nameof(right)} must meet."); }

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

		/// <summary>
		/// Adds two Intervals and returns the sum.
		/// </summary>
		/// <param name="augend">The augend.</param>
		/// <param name="addend">The addend.</param>
		/// <returns>The sum.</returns>
		public static Interval<T> Add(Interval<T> augend, Interval<T> addend) => new Interval<T>((augend.Min + addend.Min), (augend.Max + addend.Max));

		/// <summary>
		/// Subtracts two Intervals and returns the difference.
		/// </summary>
		/// <param name="minuend">The minuend.</param>
		/// <param name="subtrahend">The subtrahend.</param>
		/// <returns>The difference.</returns>
		public static Interval<T> Subtract(Interval<T> minuend, Interval<T> subtrahend) => new Interval<T>((minuend.Min - subtrahend.Max), (minuend.Max - subtrahend.Min));

		/// <summary>
		/// Divides two Intervals and returns the quotient.
		/// </summary>
		/// <param name="dividend">The dividend.</param>
		/// <param name="divisor">The divisor.</param>
		/// <returns>The quotient.</returns>
		public static Interval<T> Divide(Interval<T> dividend, Interval<T> divisor) => new Interval<T>((dividend.Min / divisor.Max), (dividend.Max / divisor.Min));

		/// <summary>
		/// Multiplies two Intervals and returns the product.
		/// </summary>
		/// <param name="multiplicand">The multiplicand.</param>
		/// <param name="multiplier">The multiplier.</param>
		/// <returns>The product.</returns>
		public static Interval<T> Multiply(Interval<T> multiplicand, Interval<T> multiplier) =>
			new Interval<T>(
				CollectionMin(multiplicand.Min * multiplier.Min, multiplicand.Min * multiplier.Max, multiplicand.Max * multiplier.Min, multiplicand.Max * multiplier.Max),
				CollectionMax(multiplicand.Min * multiplier.Min, multiplicand.Min * multiplier.Max, multiplicand.Max * multiplier.Min, multiplicand.Max * multiplier.Max)
			);

		/// <summary>
		/// Clones the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A copy of the Interval.</returns>
		public static Interval<T> Clone(Interval<T> value) => new Interval<T>(value.Min, value.Max);

		private static T CollectionMin(params T[] elements) { return elements.Min(); }
		private static T CollectionMax(params T[] elements) { return elements.Max(); }

		#endregion

		#region Operators

		#region Arithmetic Operators

		/// <summary>
		/// Adds two Intervals together and returns their sum.
		/// </summary>
		/// <param name="augend">The augend.</param>
		/// <param name="addend">The addend.</param>
		/// <returns>The sum.</returns>
		public static Interval<T> operator +(Interval<T> augend, Interval<T> addend) => Interval<T>.Add(augend, addend);

		/// <summary>
		/// Subtracts two Intervals and returns their difference.
		/// </summary>
		/// <param name="minuend">The minuend.</param>
		/// <param name="subtrahend">The subtrahend.</param>
		/// <returns>The difference.</returns>
		public static Interval<T> operator -(Interval<T> minuend, Interval<T> subtrahend) => Interval<T>.Subtract(minuend, subtrahend);

		/// <summary>
		/// Multiplies two Intervals and returns the product.
		/// </summary>
		/// <param name="multiplicand">The multiplicand.</param>
		/// <param name="multiplier">The multiplier.</param>
		/// <returns>The product.</returns>
		public static Interval<T> operator *(Interval<T> multiplicand, Interval<T> multiplier) => Interval<T>.Multiply(multiplicand, multiplier);

		/// <summary>
		/// Divides two Intervals and returns the quotient.
		/// </summary>
		/// <param name="dividend">The dividend.</param>
		/// <param name="divisor">The divisor.</param>
		/// <returns>The quotient.</returns>
		public static Interval<T> operator /(Interval<T> dividend, Interval<T> divisor) => Interval<T>.Divide(dividend, divisor);

		#endregion

		#region Comparison Operators

		/// <summary>
		/// Compares two values to determine equality.
		/// </summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool operator ==(Interval<T> left, Interval<T> right) => Interval<T>.Equals(left, right);

		/// <summary>
		/// Compares two values to determine inequality.
		/// </summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool operator !=(Interval<T> left, Interval<T> right) => !Interval<T>.Equals(left, right);

		/// <summary>
		/// Implements the &lt; operator.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator <(Interval<T> left, Interval<T> right) => (left.Min <= right.Min) && (left.Max < right.Max);

		/// <summary>
		/// Compares two values to determine which is greater.
		/// </summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool operator >(Interval<T> left, Interval<T> right) => (right.Max >= left.Max) && (right.Min > left.Min);

		/// <summary>
		/// Implements the &lt;= operator.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator <=(Interval<T> left, Interval<T> right) => (left < right) || (left.Max == right.Max);

		/// <summary>
		/// Compares two values to determine which is greater or equal.
		/// </summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> is greater than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool operator >=(Interval<T> left, Interval<T> right) => (right > left) || (right.Min == left.Min);

		#endregion

		#region Conversion Operators

		/// <summary>
		/// Performs an implicit conversion from <see cref="T"/> to <see cref="Interval{T}"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Interval<T>(T value) => new Interval<T>(value);

		#endregion

		#endregion

		#region Equality / CompareTo


		/// <summary>
		/// Compares two values for equality.
		/// </summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool Equals(Interval<T> left, Interval<T> right)
		{
			return (left.Min == right.Min && left.Max == right.Max);
		}

		/// <summary>
		/// Compares this instance with another for equality.
		/// </summary>
		/// <param name="other">The other value to compare with.</param>
		/// <returns><see langword="true" /> if this instance is equal to <paramref name="other" />; otherwise, <see langword="false" />.</returns>
		public bool Equals(Interval<T> other)
		{
			return this.Equals(other);
		}

		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
		/// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="obj" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="obj" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="obj" /> in the sort order.</description></item></list></returns>
		public int CompareTo(object obj)
		{
			return this.CompareTo(obj as Interval<T>);
		}

		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="other">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
		/// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list></returns>
		/// <exception cref="System.Exception"></exception>
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

		/// <summary>
		/// Determines whether this interval instance contains within it another interval.
		/// </summary>
		/// <param name="value">The other interval to check if it is contained within this instance interval.</param>
		/// <returns><c>true</c> if this interval instance contains the specified interval; otherwise, <c>false</c>.</returns>
		public bool Contains(T value)
		{
			return (value >= this.Min && value <= this.Max);
		}

		/// <summary>
		/// Determines whether two Intervals are disjoint.
		/// Two Intervals are disjoint if no part of one interval is contained in the other.
		/// </summary>
		/// <param name="left">The left interval.</param>
		/// <param name="right">The right interval.</param>
		/// <returns><c>true</c> if the no part of the left interval contains in the other; otherwise, <c>false</c>.</returns>
		public static bool IsDisjoint(Interval<T> left, Interval<T> right)
		{
			return !(left.Contains(right.Min) || left.Contains(right.Max));
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			return Interval<T>.Equals(this, obj as Interval<T>);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public override int GetHashCode()
		{
			return new Tuple<T, T>(this.Min, this.Max).GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>A <see cref="System.String" /> that represents this instance.</returns>
		public override string ToString()
		{
			return ToString(null, NumberFormatInfo.CurrentInfo);
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <param name="format">The format to use.
		/// -or-
		/// A null reference (<see langword="Nothing" /> in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
		/// <param name="formatProvider">The provider to use to format the value.
		/// -or-
		/// A null reference (<see langword="Nothing" /> in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system.</param>
		/// <returns>A <see cref="System.String" /> that represents this instance.</returns>
		public string ToString(string format, IFormatProvider formatProvider)
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
