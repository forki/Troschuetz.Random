/*
 * Copyright � 2006 Stefan Trosch�tz (stefan@troschuetz.de)
 * Copyright � 2012-2016 Alessio Parma (alessio.parma@gmail.com)
 *
 * This file is part of Troschuetz.Random Class Library.
 *
 * Troschuetz.Random is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * See the GNU Lesser General Public License for more details.
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

namespace Troschuetz.Random.Distributions.Continuous
{
    using Core;
    using Generators;
    using PommaLabs.Thrower;
    using System;
    using System.Diagnostics;

    /// <summary>
    ///   Provides generation of gamma distributed random numbers.
    /// </summary>
    /// <remarks>
    ///   The implementation of the <see cref="GammaDistribution"/> type bases upon information
    ///   presented on <a href="http://en.wikipedia.org/wiki/Gamma_distribution">Wikipedia - Gamma distribution</a>.
    /// </remarks>
    [Serializable]
    public class GammaDistribution<TGen> : AbstractDistribution<TGen>, IContinuousDistribution, IAlphaDistribution<double>, IThetaDistribution<double>
        where TGen : IGenerator
    {
        #region Constants

        /// <summary>
        ///   The default value assigned to <see cref="Alpha"/> if none is specified.
        /// </summary>
        public const double DefaultAlpha = 1;

        /// <summary>
        ///   The default value assigned to <see cref="Theta"/> if none is specified.
        /// </summary>
        public const double DefaultTheta = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        ///   Stores the parameter alpha which is used for generation of gamma distributed random numbers.
        /// </summary>
        double _alpha;

        /// <summary>
        ///   Stores the parameter theta which is used for generation of gamma distributed random numbers.
        /// </summary>
        double _theta;

        /// <summary>
        ///   Gets or sets the parameter alpha which is used for generation of gamma distributed
        ///   random numbers.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="value"/> is less than or equal to zero.
        /// </exception>
        /// <remarks>
        ///   Calls <see cref="AreValidParams"/> to determine whether a value is valid and therefore assignable.
        /// </remarks>
        public double Alpha
        {
            get { return _alpha; }
            set
            {
                Raise<ArgumentOutOfRangeException>.IfNot(IsValidAlpha(value), ErrorMessages.InvalidParams);
                _alpha = value;
            }
        }

        /// <summary>
        ///   Gets or sets the parameter theta which is used for generation of gamma distributed
        ///   random numbers.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="value"/> is less than or equal to zero.
        /// </exception>
        /// <remarks>
        ///   Calls <see cref="AreValidParams"/> to determine whether a value is valid and therefore assignable.
        /// </remarks>
        public double Theta
        {
            get { return _theta; }
            set
            {
                Raise<ArgumentOutOfRangeException>.IfNot(IsValidTheta(value), ErrorMessages.InvalidParams);
                _theta = value;
            }
        }

        #endregion Fields

        #region Construction

        /// <summary>
        ///   Initializes a new instance of the <see cref="GammaDistribution"/> class, using the
        ///   specified <see cref="IGenerator"/> as underlying random number generator.
        /// </summary>
        /// <param name="generator">An <see cref="IGenerator"/> object.</param>
        /// <param name="alpha">
        ///   The parameter alpha which is used for generation of gamma distributed random numbers.
        /// </param>
        /// <param name="theta">
        ///   The parameter theta which is used for generation of gamma distributed random numbers.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="alpha"/> or <paramref name="theta"/> are less than or equal to zero.
        /// </exception>
        public GammaDistribution(TGen generator, double alpha, double theta) : base(generator)
        {
            Raise<ArgumentOutOfRangeException>.IfNot(AreValidParams(alpha, theta), ErrorMessages.InvalidParams);
            _alpha = alpha;
            _theta = theta;
        }

        #endregion Construction

        #region Instance Methods

        /// <summary>
        ///   Determines whether the specified value is valid for parameter <see cref="Alpha"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><see langword="true"/> if value is greater than 0.0; otherwise, <see langword="false"/>.</returns>
        public bool IsValidAlpha(double value) => AreValidParams(value, Theta);

        /// <summary>
        ///   Determines whether the specified value is valid for parameter <see cref="Theta"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><see langword="true"/> if value is greater than 0.0; otherwise, <see langword="false"/>.</returns>
        public bool IsValidTheta(double value) => AreValidParams(_alpha, value);

        #endregion Instance Methods

        #region IContinuousDistribution Members

        /// <summary>
        ///   Gets the minimum possible value of distributed random numbers.
        /// </summary>
        public double Minimum => 0.0;

        /// <summary>
        ///   Gets the maximum possible value of distributed random numbers.
        /// </summary>
        public double Maximum => double.PositiveInfinity;

        /// <summary>
        ///   Gets the mean of distributed random numbers.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///   Thrown if mean is not defined for given distribution with some parameters.
        /// </exception>
        public double Mean => _alpha * Theta;

        /// <summary>
        ///   Gets the median of distributed random numbers.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///   Thrown if median is not defined for given distribution with some parameters.
        /// </exception>
        public double Median
        {
            get { throw new NotSupportedException(ErrorMessages.UndefinedMedian); }
        }

        /// <summary>
        ///   Gets the variance of distributed random numbers.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///   Thrown if variance is not defined for given distribution with some parameters.
        /// </exception>
        public double Variance => _alpha * Sqr(Theta);

        /// <summary>
        ///   Gets the mode of distributed random numbers.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///   Thrown if mode is not defined for given distribution with some parameters.
        /// </exception>
        public double[] Mode
        {
            get
            {
                if (_alpha >= 1.0)
                {
                    return new[] { (_alpha - 1.0) * Theta };
                }
                throw new NotSupportedException(ErrorMessages.UndefinedModeForParams);
            }
        }

        /// <summary>
        ///   Returns a distributed floating point random number.
        /// </summary>
        /// <returns>A distributed double-precision floating point number.</returns>
        public double NextDouble() => Sample(TypedGenerator, _alpha, _theta);

        #endregion IContinuousDistribution Members

        #region TRandom Helpers

        /// <summary>
        ///   Determines whether gamma distribution is defined under given parameters. The default
        ///   definition returns true if alpha and theta are greater than zero; otherwise, it
        ///   returns false.
        /// </summary>
        /// <remarks>
        ///   This is an extensibility point for the <see cref="GammaDistribution{TGen}"/> class.
        /// </remarks>
        public static Func<double, double, bool> AreValidParams { get; set; } = (alpha, theta) =>
        {
            return alpha > 0.0 && theta > 0.0;
        };

        /// <summary>
        ///   Declares a function returning a gamma distributed floating point random number.
        /// </summary>
        /// <remarks>
        ///   This is an extensibility point for the <see cref="GammaDistribution{TGen}"/> class.
        /// </remarks>
        public static Func<TGen, double, double, double> Sample { get; set; } = (generator, alpha, theta) =>
        {
            var helper1 = alpha - Math.Floor(alpha);
            var helper2 = Math.E / (Math.E + helper1);
            double xi, eta;
            do
            {
                var gen1 = 1.0 - generator.NextDouble();
                var gen2 = 1.0 - generator.NextDouble();
                if (gen1 <= helper2)
                {
                    xi = Math.Pow(gen1 / helper2, 1.0 / helper1);
                    eta = gen2 * Math.Pow(xi, helper1 - 1.0);
                }
                else
                {
                    xi = 1.0 - Math.Log((gen1 - helper2) / (1.0 - helper2));
                    eta = gen2 * Math.Pow(Math.E, -xi);
                }
            } while (eta > Math.Pow(xi, helper1 - 1.0) * Math.Pow(Math.E, -xi));

            for (var i = 1; i <= alpha; i++)
            {
                xi -= Math.Log(generator.NextDouble());
            }

            return xi * theta;
        };

        #endregion TRandom Helpers
    }

    /// <summary>
    ///   Provides generation of gamma distributed random numbers.
    /// </summary>
    /// <remarks>
    ///   The implementation of the <see cref="GammaDistribution"/> type bases upon information
    ///   presented on <a href="http://en.wikipedia.org/wiki/Gamma_distribution">Wikipedia - Gamma distribution</a>.
    /// </remarks>
    [Serializable]
    public sealed class GammaDistribution : GammaDistribution<IGenerator>
    {
        #region Construction

        /// <summary>
        ///   Initializes a new instance of the <see cref="GammaDistribution"/> class, using a
        ///   <see cref="NumericalRecipes3Q1Generator"/> as underlying random number generator.
        /// </summary>
        public GammaDistribution()
            : base(new NumericalRecipes3Q1Generator(), DefaultAlpha, DefaultTheta)
        {
            Debug.Assert(Generator is NumericalRecipes3Q1Generator);
            Debug.Assert(Equals(Alpha, DefaultAlpha));
            Debug.Assert(Equals(Theta, DefaultTheta));
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="GammaDistribution"/> class, using a
        ///   <see cref="NumericalRecipes3Q1Generator"/> with the specified seed value.
        /// </summary>
        /// <param name="seed">
        ///   An unsigned number used to calculate a starting value for the pseudo-random number sequence.
        /// </param>
        public GammaDistribution(uint seed)
            : base(new NumericalRecipes3Q1Generator(seed), DefaultAlpha, DefaultTheta)
        {
            Debug.Assert(Generator is NumericalRecipes3Q1Generator);
            Debug.Assert(Generator.Seed == seed);
            Debug.Assert(Equals(Alpha, DefaultAlpha));
            Debug.Assert(Equals(Theta, DefaultTheta));
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="GammaDistribution"/> class, using the
        ///   specified <see cref="IGenerator"/> as underlying random number generator.
        /// </summary>
        /// <param name="generator">An <see cref="IGenerator"/> object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        public GammaDistribution(IGenerator generator)
            : base(generator, DefaultAlpha, DefaultTheta)
        {
            Debug.Assert(ReferenceEquals(Generator, generator));
            Debug.Assert(Equals(Alpha, DefaultAlpha));
            Debug.Assert(Equals(Theta, DefaultTheta));
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="GammaDistribution"/> class, using a
        ///   <see cref="NumericalRecipes3Q1Generator"/> as underlying random number generator.
        /// </summary>
        /// <param name="alpha">
        ///   The parameter alpha which is used for generation of gamma distributed random numbers.
        /// </param>
        /// <param name="theta">
        ///   The parameter theta which is used for generation of gamma distributed random numbers.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="alpha"/> or <paramref name="theta"/> are less than or equal to zero.
        /// </exception>
        public GammaDistribution(double alpha, double theta)
            : base(new NumericalRecipes3Q1Generator(), alpha, theta)
        {
            Debug.Assert(Generator is NumericalRecipes3Q1Generator);
            Debug.Assert(Equals(Alpha, alpha));
            Debug.Assert(Equals(Theta, theta));
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="GammaDistribution"/> class, using a
        ///   <see cref="NumericalRecipes3Q1Generator"/> with the specified seed value.
        /// </summary>
        /// <param name="seed">
        ///   An unsigned number used to calculate a starting value for the pseudo-random number sequence.
        /// </param>
        /// <param name="alpha">
        ///   The parameter alpha which is used for generation of gamma distributed random numbers.
        /// </param>
        /// <param name="theta">
        ///   The parameter theta which is used for generation of gamma distributed random numbers.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="alpha"/> or <paramref name="theta"/> are less than or equal to zero.
        /// </exception>
        public GammaDistribution(uint seed, double alpha, double theta)
            : base(new NumericalRecipes3Q1Generator(seed), alpha, theta)
        {
            Debug.Assert(Generator is NumericalRecipes3Q1Generator);
            Debug.Assert(Generator.Seed == seed);
            Debug.Assert(Equals(Alpha, alpha));
            Debug.Assert(Equals(Theta, theta));
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="GammaDistribution"/> class, using the
        ///   specified <see cref="IGenerator"/> as underlying random number generator.
        /// </summary>
        /// <param name="generator">An <see cref="IGenerator"/> object.</param>
        /// <param name="alpha">
        ///   The parameter alpha which is used for generation of gamma distributed random numbers.
        /// </param>
        /// <param name="theta">
        ///   The parameter theta which is used for generation of gamma distributed random numbers.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="alpha"/> or <paramref name="theta"/> are less than or equal to zero.
        /// </exception>
        public GammaDistribution(IGenerator generator, double alpha, double theta) : base(generator, alpha, theta)
        {
            Debug.Assert(ReferenceEquals(Generator, generator));
            Debug.Assert(Equals(Alpha, alpha));
            Debug.Assert(Equals(Theta, theta));
        }

        #endregion Construction
    }
}
