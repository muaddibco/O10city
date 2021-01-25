using System.Collections.Generic;
using O10.Core.Architecture;

using O10.Core.Exceptions;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace O10.Core.HashCalculations
{
    [RegisterDefaultImplementation(typeof(IHashCalculationsRepository), Lifetime = LifetimeManagement.Singleton)]
    public class HashCalculationRepository : IHashCalculationsRepository
    {
        private readonly Dictionary<HashType, Stack<IHashCalculation>> _hashCalculations;
        private readonly IServiceProvider _serviceProvider;
        private readonly object _sync = new object();

		public HashCalculationRepository(IEnumerable<IHashCalculation> hashCalculations, IServiceProvider serviceProvider)
		{
			_hashCalculations = new Dictionary<HashType, Stack<IHashCalculation>>();
			_serviceProvider = serviceProvider;

			if (hashCalculations != null)
			{
				foreach (IHashCalculation calculation in hashCalculations)
				{
					if (!_hashCalculations.ContainsKey(calculation.HashType))
					{
						_hashCalculations.Add(calculation.HashType, new Stack<IHashCalculation>());
					}

					_hashCalculations[calculation.HashType].Push(calculation);
				}
			}
        }

        public IHashCalculation Create(HashType key)
        {
            if (!_hashCalculations.ContainsKey(key))
            {
                throw new HashAlgorithmNotSupportedException(key);
            }

            lock (_sync)
            {
                if (_hashCalculations[key].Count > 1)
                {
                    return _hashCalculations[key].Pop();
                }

                IHashCalculation calculationTemp = _hashCalculations[key].Pop();
                IHashCalculation calculation = (IHashCalculation)ActivatorUtilities.CreateInstance(_serviceProvider, calculationTemp.GetType());
                _hashCalculations[key].Push(calculationTemp);
                return calculation;
            }
        }

        public void Utilize(IHashCalculation obj)
        {
            if (!_hashCalculations.ContainsKey(obj.HashType))
            {
                throw new HashAlgorithmNotSupportedException(obj.HashType);
            }

            _hashCalculations[obj.HashType].Push(obj);
        }
    }
}
