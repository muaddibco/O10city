// SPDX-License-Identifier: MIT
pragma solidity ^0.6.2;
pragma experimental ABIEncoderV2;

import '@openzeppelin/contracts/token/ERC721/ERC721.sol';
import '@openzeppelin/contracts/token/ERC721/ERC721Burnable.sol';
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Counters.sol";

contract O10ElectionCommittee is ERC721, ERC721Burnable, Ownable {
    using Counters for Counters.Counter;
    using EnumerableSet for EnumerableSet.UintSet;

    struct CandidateDetails {
        string Name;
        bytes32 AssetId;
    }

    struct Candidate {
        uint256 Index;
        bool IsActive;
        CandidateDetails Details;
    }

    struct Poll {
        bool IsRegistered;
        uint256 Index;
        string Name;

        Candidate[] Candidates;
    }

    Counters.Counter PollsCounter;
    Counters.Counter CandidatesCounter;

    mapping(string => Poll) _polls;
    EnumerableSet.UintSet _pollIndicies;

    constructor(string memory name, string memory symbol) ERC721(name, symbol) public {}

    function registerPoll(string memory name, CandidateDetails[] memory candidates) public returns(uint256) {
        Poll storage poll = _polls[name];
        require(!poll.IsRegistered, "Poll with this name already registered");
        poll.IsRegistered = true;
        poll.Name = name;
        for (uint256 i = 0; i < candidates.length; i++) {
            for(uint256 j = 0; j < poll.Candidates.length; j++) {
                require(keccak256(abi.encodePacked(poll.Candidates[j].Details.Name)) != keccak256(abi.encodePacked(candidates[i].Name)) && poll.Candidates[j].Details.AssetId != candidates[i].AssetId, "One of candidates has duplicated details");
            }
            uint256 idx = CandidatesCounter.current();
            poll.Candidates.push(Candidate({Index: idx, IsActive: true, Details: candidates[i]}));
            CandidatesCounter.increment();
        }

        poll.Index = PollsCounter.current();
        PollsCounter.increment();

        return poll.Index;
    }
}