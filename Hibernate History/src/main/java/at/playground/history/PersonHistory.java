package at.playground.history;

import at.playground.Request;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.Filter;
import org.hibernate.annotations.FilterDef;
import org.hibernate.annotations.ParamDef;

import javax.persistence.*;
import java.time.Instant;
import java.util.List;

@Data
@NoArgsConstructor
@AllArgsConstructor
@Entity
@Table(name = "persons_history")
@FilterDef(name = "HistoryFilter", parameters = { @ParamDef(name = "filterDate", type = "java.time.Instant") })
@Filter(name = "HistoryFilter", condition = "history_valid_from <= :filterDate and (history_valid_to is null or :filterDate < history_valid_to)")
public class PersonHistory {
    @Id
    private Long id;

    @Column(name = "first_name", nullable = false, length = 255)
    private String firstName;

    @Column(name = "last_name", nullable = false, length = 255)
    private String lastName;

    @OneToMany(mappedBy = "person", cascade = CascadeType.ALL)
    @Filter(name = "HistoryFilter", condition = "history_valid_from <= :filterDate and (history_valid_to is null or :filterDate < history_valid_to)")
    private List<NoticeHistory> notices;

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