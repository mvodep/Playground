package at.playground.history;

import at.playground.Request;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import javax.persistence.*;
import java.time.Instant;

@Data
@NoArgsConstructor
@AllArgsConstructor
@Entity
@Table(name = "notices_history")
public class NoticeHistory {
    @Id
    private Long id;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "person_id")
    private PersonHistory person;

    @Column(name = "notice", nullable = false, length = 255)
    private String notice;

    @ManyToOne(cascade = CascadeType.ALL)
    @JoinColumn(name="changed_by_request_id", nullable = false)
    private Request changedByRequest;

    @ManyToOne(cascade = CascadeType.ALL)
    @JoinColumn(name="deleted_by_request_id")
    private Request deleteByRequest;

    @Column(name = "history_valid_from", nullable = false)
    private Instant historyValidFrom;

    @Column(name = "history_valid_to")
    private Instant historyValidTo;
}